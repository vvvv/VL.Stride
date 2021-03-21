using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Stride.Graphics;

namespace VL.Stride.Spout
{
    public class SpoutSender : SpoutThing
    {
        public SpoutSender(string senderName, Texture srcTexture)
            :base()
        {
            SenderName = senderName;
            this.frame = srcTexture;
            textureDesc = new TextureDesc(Frame);
        }

        public void Initialize()
        {

            UpdateMaxSenders();
            if (AddNameToSendersList(senderName))
            {
                byte[] desc = textureDesc.ToByteArray();
                SenderDescriptionMap = MemoryMappedFile.CreateOrOpen(SenderName, desc.Length);
                using (var mmvs = SenderDescriptionMap.CreateViewStream())
                {
                    mmvs.Write(desc, 0, desc.Length);
                }

                //If we are the first/only sender, create a new ActiveSenderName map.
                //This is a separate shared memory containing just a sender name
                //that receivers can use to retrieve the current active Sender.
                ActiveSenderMap = MemoryMappedFile.CreateOrOpen(ActiveSenderMMF, SenderNameLength);
                using (var mmvs = ActiveSenderMap.CreateViewStream())
                {
                    var firstByte = mmvs.ReadByte();
                    if (firstByte == 0) //no active sender yet
                    {
                        mmvs.Position = 0;
                        mmvs.Write(GetNameBytes(SenderName), 0, SenderNameLength);
                    }
                }

            }
        }

        bool AddNameToSendersList(string name)
        {
            bool createdNew;
            Mutex mutex = new Mutex(true, SenderNamesMMF + "_mutex", out createdNew);
            if (mutex == null)
                return false;
            bool success = false;
            try
            {
                if (mutex.WaitOne(SpoutWaitTimeout))
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (AbandonedMutexException e)
            {
                success = true;    
            }
            finally
            {
                if (success)
                {
                    List<string> senders = GetSenderNames();
                    if (senders.Contains(this.senderName))
                    {
                        success = false;
                    }
                    else
                    {
                        senders.Add(name);
                        WriteSenderNamesToMMF(senders);
                    }
                }
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
            return success;
        }

        void RemoveNameFromSendersList()
        {
            bool createdNew;
            Mutex mutex = new Mutex(true, SenderNamesMMF + "_mutex", out createdNew);
            if (mutex == null)
                return;
            try
            {
                mutex.WaitOne(SpoutWaitTimeout);
            }
            catch (AbandonedMutexException e)
            {
                //Log.Add(e);     
            }
            finally
            {
                List<string> senders = GetSenderNames();
                if (senders.Contains(this.senderName))
                {
                    senders.Remove(senderName);
                    WriteSenderNamesToMMF(senders);
                }
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }

        void WriteSenderNamesToMMF(List<string> senders)
        {
            int len = SenderNameLength * MaxSenders;
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(SenderNamesMMF, len);
            MemoryMappedViewStream mmvs = mmf.CreateViewStream();
            int count = 0;
            for (int i = 0; i < senders.Count; i++)
            {
                byte[] nameBytes = GetNameBytes(senders[i]);
                mmvs.Write(nameBytes, 0, nameBytes.Length);
                count += nameBytes.Length;
            }
            byte[] b = new byte[len - count];
            mmvs.Write(b, 0, b.Length);
            mmvs.Dispose();
            mmf.Dispose();
        }

        public override void Dispose()
        {
            UpdateMaxSenders();
            RemoveNameFromSendersList();
            base.Dispose();
        }
    }
}

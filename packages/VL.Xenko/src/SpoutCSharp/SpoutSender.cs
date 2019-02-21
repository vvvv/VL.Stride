using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Xenko.Graphics;

namespace VL.Xenko.Spout
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
            if (AddNameToSendersList(senderName))
            {
                sharedMemory = MemoryMappedFile.CreateNew(SenderName, 280);
                sharedMemoryStream = sharedMemory.CreateViewStream();
                byte[] nameBytes = Encoding.Unicode.GetBytes(SenderName);
                Array.Copy(nameBytes, 0, textureDesc.Description, 0, nameBytes.Length);
                byte[] desc = textureDesc.ToByteArray();
                sharedMemoryStream.Write(desc, 0, desc.Length);
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
            RemoveNameFromSendersList();
            base.Dispose();
        }
    }
}

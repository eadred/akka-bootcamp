using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail
{
    public class TailActor : UntypedActor
    {
        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            var fullPath = Path.GetFullPath(filePath);
            _observer = new FileObserver(Self, fullPath);
            _observer.Start();

            _fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);
            var text = _fileStreamReader.ReadToEnd();

            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if(message is FileError)
            {
                var msg = message as FileError;
                _reporterActor.Tell(string.Format("Tail error: {0}", msg.Reason));
            }
            else if(message is InitialRead)
            {
                var msg = message as InitialRead;
                _reporterActor.Tell(msg.Text);
            }
        }

        public class FileWrite
        {
            public string FileName { get; private set; }

            public FileWrite(string fileName)
            {
                FileName = fileName;
            }
        }

        public class FileError
        {
            public string FileName { get; private set; }
            public string Reason { get; private set; }

            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }
        }

        public class InitialRead
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }

            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }
    }
}

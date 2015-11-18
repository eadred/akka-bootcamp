using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTail.Messages.Error;
using WinTail.Messages.Success;
using WinTail.Messages.System;

namespace WinTail
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        private readonly IActorRef _tailCoordinatorActor;

        public FileValidatorActor(IActorRef consoleWriterActor, IActorRef tailCoordinatorActor)
        {
            _consoleWriterActor = consoleWriterActor;
            _tailCoordinatorActor = tailCoordinatorActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                _consoleWriterActor.Tell(new NullInputError("Input was blank. Please try again.\n"));

                Sender.Tell(new ContinueProcessing());
            }
            else
            {
                if (IsFileUri(msg))
                {
                    _consoleWriterActor.Tell(new InputSuccess(string.Format("Starting processing for {0}", msg)));

                    _tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    _consoleWriterActor.Tell(new ValidationError(string.Format("{0} is not an existing URI on disk.", msg)));

                    Sender.Tell(new ContinueProcessing());
                }
            }
        }

        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}

namespace Screenshoter.Command{

    /// <summary> Command that allows the user to close the program.</summary>
    public class ExitCommand : CommandBase{

        public ExitCommand() : base("exit", "Closes the program."){}

        /// <inheritdoc/>
        public override void OnSendEvent(string[] args){
            Program.Exit();
        }
    }
}
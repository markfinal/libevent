using Bam.Core;
namespace libevent
{
    [ModuleGroup("Thirdparty/libevent")]
    sealed class GenerateConfigHeader :
        C.CModule
    {
        public static Bam.Core.PathKey Key = Bam.Core.PathKey.Generate("libevent conf header");

        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);
            this.GeneratedPaths.Add(Key, this.CreateTokenizedString("$(packagebuilddir)/event2/event-config.h"));
        }

        public override void
        Evaluate()
        {
            this.ReasonToExecute = null;
            var outputPath = this.GeneratedPaths[Key].Parse();
            if (!System.IO.File.Exists(outputPath))
            {
                this.ReasonToExecute = Bam.Core.ExecuteReasoning.FileDoesNotExist(this.GeneratedPaths[Key]);
                return;
            }
        }

        protected override void
        ExecuteInternal(
            ExecutionContext context)
        {
            var destPath = this.GeneratedPaths[Key].Parse();
            var destDir = System.IO.Path.GetDirectoryName(destPath);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
            {
                using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
                {
                    using (System.IO.TextReader readFile = new System.IO.StreamReader(this.CreateTokenizedString("$(packagedir)/WIN32-Code/nmake/event2/event-config.h").Parse()))
                    {
                        writeFile.Write(readFile.ReadToEnd());
                    }
                }
            }
            else
            {
                using (System.IO.TextWriter writeFile = new System.IO.StreamWriter(destPath))
                {
                    writeFile.WriteLine("#define _EVENT_SIZEOF_SIZE_T 8"); // assume 64-bit
                    if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
                    {
                        writeFile.WriteLine("#include \"BaseTsd.h\"");
                        writeFile.WriteLine("#define _EVENT_ssize_t SSIZE_T"); // signed size_t
                    }
                }
            }
            Log.Info("Writing libevent configuration header : {0}", destPath);
        }

        protected override void
        GetExecutionPolicy(
            string mode)
        {
            // TODO: do nothing
        }
    }
}

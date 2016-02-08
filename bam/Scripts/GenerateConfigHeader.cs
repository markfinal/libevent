using Bam.Core;
namespace libevent
{
    [ModuleGroup("Thirdparty/libevent")]
    sealed class GenerateConfigHeader :
        C.ProceduralHeaderFile
    {
        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/event2/event-config.h");
            }
        }

        protected override string Contents
        {
            get
            {
                if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
                {
                    using (System.IO.TextReader readFile = new System.IO.StreamReader(this.CreateTokenizedString("$(packagedir)/WIN32-Code/nmake/event2/event-config.h").Parse()))
                    {
                        return readFile.ReadToEnd();
                    }
                }
                else
                {
                    return "#define _EVENT_SIZEOF_SIZE_T 8"; // assume 64-bit
                }
            }
        }
    }
}

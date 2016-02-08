using Bam.Core;
namespace libevent
{
    [ModuleGroup("Thirdparty/libevent")]
    class GeneratePrivateConfigHeader :
        C.ProceduralHeaderFile
    {
        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/evconfig-private.h");
            }
        }

        protected override string Contents
        {
            get
            {
                if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
                {
                    using (System.IO.TextReader readFile = new System.IO.StreamReader(this.CreateTokenizedString("$(packagedir)/WIN32-Code/nmake/evconfig-private.h").Parse()))
                    {
                        return readFile.ReadToEnd();
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}

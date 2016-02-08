using Bam.Core;
namespace libevent
{
    [ModuleGroup("Thirdparty/libevent")]
    class GenerateConfigHeader :
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
                    var contents = new System.Text.StringBuilder();
                    contents.AppendLine("#define _EVENT_SIZEOF_SIZE_T 8"); // assume 64-bit
                    contents.AppendLine("#define EVENT__SIZEOF_SIZE_T 8");
                    contents.AppendLine("#define EVENT__HAVE_UINT64_T");
                    contents.AppendLine("#define EVENT__HAVE_UINT32_T");
                    contents.AppendLine("#define EVENT__HAVE_UINT16_T");
                    contents.AppendLine("#define EVENT__HAVE_SYS_TIME_H");
                    contents.AppendLine("#define EVENT__HAVE_STDINT_H");
                    contents.AppendLine("#define EVENT__HAVE_SA_FAMILY_T");
                    contents.AppendLine("#define EVENT__HAVE_NETINET_IN_H");
                    //contents.AppendLine("#define EVENT__HAVE_NETINET_IN6_H");
                    contents.AppendLine("#define EVENT__HAVE_STRUCT_SOCKADDR_STORAGE");
                    contents.AppendLine("#define EVENT__HAVE_STRUCT_IN6_ADDR");
                    contents.AppendLine("#define EVENT__HAVE_STRUCT_SOCKADDR_IN6");
                    contents.AppendLine("#define EVENT__HAVE_UNISTD_H");
                    contents.AppendLine("#define EVENT__HAVE_SYS_STAT_H");
                    contents.AppendLine("#define __USE_XOPEN2K"); // so that gethostname is defined in unistd.h // TODO: does not work later on
                    //contents.AppendLine("#define __USE_POSIX199309"); // for siginfo_t
                    contents.AppendLine("#define EVENT__HAVE_SIGNAL_H");
                    return contents.ToString();
                }
            }
        }
    }
}

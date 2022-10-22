using System.Text;
using System.Windows;

namespace LyricEditor
{
    public partial class App : Application
    {
        public App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}

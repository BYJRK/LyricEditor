using System;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace LyricEditor
{
    public partial class App : Application
    {
        public App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 先读取配置文件，再启动主窗口
            ResourceDictionary resourceDictionary = new ResourceDictionary();
            // 读取字体：
            String fontName = ConfigurationManager.AppSettings["LyricFont"];
            resourceDictionary.Add("LyricFont", new FontFamily(fontName));
            // 读取字体大小：
            Double fontSize = Double.Parse(ConfigurationManager.AppSettings["LyricFontSize"]);
            resourceDictionary.Add("LyricFontSize", fontSize);
            this.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}

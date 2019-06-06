using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CefSharp;

namespace SuningHelper
{
    public static class EvaluateScriptHelper
    {
        public static async Task<bool> OpenLogicPage(this IFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            const string script =
                @"(function ()
                {
                    var success = true;
                    var logiclink = document.getElementsByName('pctbsy_index_ljdl_01');
                    if ((logiclink != null) && (logiclink[0] != null))
                        logiclink[0].click();
                    return success;
                })();";

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            return (bool)response.Result;
        }

        public static async Task<bool> InputTongGangKami(this IFrame frame, string kami)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            string script =
                @"(function ()
                    {
                        var success = true;
                    
                        //输入卡密
                        var input = document.getElementsByName('copperCode');
                        if ((input != null) && (input[0] != null))
                            input[0].value = '##ID1##';

                        //立即兑换
                        var duihuanbtn = document.getElementsByName('pctbsy_index_dh_01');
                        if ((duihuanbtn != null) && (duihuanbtn[0] != null))
                            duihuanbtn[0].click();

                        return success;
                    })();";

            // For simple inline scripts you could use String.Format() but
            // beware of braces in the javascript code. If reading from a file
            // it's probably safer to include tokens that can be replaced via
            // regex.
            script = Regex.Replace(script, "##ID1##", kami);

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            return (bool)response.Result;
        }

        public static async Task<string> ReadTongGangKamiResult(this IFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentException("An IFrame instance is required.", "frame");
            }

            // Scripts should be minified for production builds. The script
            // could also be read from a file...
            string script =
                @"(function ()
                    {
                        var Success = true;
                        var Message = null;

                        //处理结果
                        //抱歉，兑换码不存在
                        //抱歉，兑换码已使用
                        var resulterrordlg = document.getElementsByClassName('dialog result Copper hide');
                        //兑换码余额100.00 手续费1.00 本月已获取10次
                        var resultokdlg = document.getElementsByClassName('dialog double Copper hide');
 
                        if ((resulterrordlg != null) && (resulterrordlg[0].style.display == 'block'))
                        {
                            var msg = resulterrordlg[0].getElementsByClassName('res');
                            if ((msg != null) && (msg[0] != null))
                                Message = msg[0].innerText;

                            var okbtn = resulterrordlg[0].getElementsByClassName('confirm');
                            if ((okbtn != null) && (okbtn[0] != null))
                                okbtn[0].click();
                        }
                        else if ((resultokdlg != null) && (resultokdlg[0].style.display == 'block'))
                        {
                            var msg = resultokdlg[0].getElementsByClassName('res');
                            if ((msg != null) && (msg[0] != null))
                                Message = msg[0].innerText;

                            var okbtn = resultokdlg[0].getElementsByClassName('confirm');
                            if ((okbtn != null) && (okbtn[0] != null))
                                okbtn[0].click();

                            Success = false;
                        }

                        return Message;
                    })();";

            var response = await frame.EvaluateScriptAsync(script);
            if (!response.Success)
            {
                throw new Exception(response.Message);
            }
            return (string)(response.Result);
        }
    }
}

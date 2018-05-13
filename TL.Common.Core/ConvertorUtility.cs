using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace TL.Common.Core
{
    /// <summary>
    /// 转换类
    /// </summary>
    public class ConvertorUtility
    {
        /// <summary>
        /// 转换类型
        /// </summary>
        /// <typeparam name="T">要转换的类型，可以为Nullable的泛型</typeparam>
        /// <param name="val">要转换的值</param>
        /// <returns></returns>
        public static T ConvertType<T>(object val)
        {
            return ConvertType<T>(val, default(T));
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        /// <typeparam name="T">要转换的类型，可以为Nullable的泛型</typeparam>
        /// <param name="val">要转换的值</param>
        /// <param name="defaultVal">转换失败默认的值</param>
        /// <returns></returns>
        public static T ConvertType<T>(object val, T defaultVal)
        {
            object returnVal = ConvertByType(val, typeof(T));
            if (returnVal == null)
                return defaultVal;
            else
            {
                return (T)returnVal;
            }
        }

        /// <summary>
        /// 通过反射获取未知类型时的类型转换
        /// 转换失败返回null
        /// </summary>
        /// <param name="val"></param>
        /// <param name="tp"></param>
        /// <returns></returns>
        public static object ConvertByType(object val, Type tp)
        {
            if (val == null) return null;
            if (tp.IsGenericType)
            {
                tp = tp.GetGenericArguments()[0];
            }
            #region 单独转换Datetime和String

            if (tp.Name.ToLower() == "datetime")
            {
                DateTime? dt = ParseDateTime(val.ToString());
                if (dt == null) return null;
                object objDt = dt.Value;
                return objDt;
            }
            if (tp.Name.ToLower() == "string")
            {
                object objStr = val.ToString();
                return objStr;
            }
            #endregion
            var TryParse = tp.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                                            new Type[] { typeof(string), tp.MakeByRefType() },
                                            new ParameterModifier[] { new ParameterModifier(2) });
            var parameters = new object[] { val.ToString(), Activator.CreateInstance(tp) };
            bool success = (bool)TryParse.Invoke(null, parameters);
            if (success)
            {
                return parameters[1];
            }
            return null;
        }

        /// <summary>
        /// 转换类型，返回是否转换成功
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="val">要转换的值</param>
        /// <param name="returnVal">返回的值</param>
        /// <returns></returns>
        public static bool TryConvert<T>(object val, out T returnVal)
        {
            returnVal = default(T);
            if (val == null) return false;
            Type tp = typeof(T);
            if (tp.IsGenericType)
            {
                tp = tp.GetGenericArguments()[0];
            }

            #region 单独转换Datetime和string

            if (tp.Name.ToLower() == "datetime")
            {
                DateTime? dt = ParseDateTime(val.ToString());
                if (dt == null) return false;
                object objDt = dt.Value;
                returnVal = (T)objDt;
                return true;
            }
            if (tp.Name.ToLower() == "string")
            {
                object objStr = val.ToString();
                returnVal = (T)objStr;
                return true;
            }
            #endregion

            var TryParse = tp.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                                            new Type[] { typeof(string), tp.MakeByRefType() },
                                            new ParameterModifier[] { new ParameterModifier(2) });
            var parameters = new object[] { val.ToString(), Activator.CreateInstance(tp) };
            bool success = (bool)TryParse.Invoke(null, parameters);
            if (success)
            {
                returnVal = (T)parameters[1];
                return true;
            }
            return false;
        }

        /// <summary>
        /// 转换为DateTime
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(object obj, DateTime defaultValue)
        {
            DateTime result = defaultValue;
            if (obj != null)
            {
                if (!DateTime.TryParse(obj.ToString().Trim(), out result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        /// <summary>
        ///  根据数据日期类型 转化日期
        ///  gqh 2012-11-28
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <param name="dateFormat">输入日期格式 比如  yyyyMMdd</param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(object obj, DateTime defaultValue, string dateFormat)
        {
            DateTime result = defaultValue;

            if (obj != null)
            {
                //日期验证
                IFormatProvider ifp = new CultureInfo("zh-TW", true);

                DateTime.TryParseExact(obj.ToString(), dateFormat, ifp, DateTimeStyles.None, out result);
            }
            return result;
        }

        /// <summary>
        /// 转换为int类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <param name="numStyle">数字格式</param>
        /// <returns></returns>
        public static int ConvertToInt(object obj, int defaultValue, NumberStyles numStyle)
        {
            int result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                if (!int.TryParse(obj.ToString().Trim(), numStyle, null, out result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        /// <summary>
        /// 转换为int类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <param name="numStyle">数字格式</param>
        /// <returns></returns>
        public static byte ConvertToByte(object obj, byte defaultValue, NumberStyles numStyle)
        {
            Byte result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                if (!Byte.TryParse(obj.ToString().Trim(), numStyle, null, out result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        /// <summary>
        /// 转换为int类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <returns></returns>
        public static int ConvertToInt(object obj, int defaultValue)
        {
            return ConvertToInt(obj, defaultValue, NumberStyles.Number);
        }

        /// <summary>
        /// 转换为int类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <returns></returns>
        public static byte ConvertToByte(object obj, Byte defaultValue)
        {
            return ConvertToByte(obj, defaultValue, NumberStyles.Number);
        }

        /// <summary>
        /// 转换为decimal类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <returns></returns>
        public static decimal ConvertToDecimal(object obj, decimal defaultValue)
        {
            decimal result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                if (!decimal.TryParse(obj.ToString().Trim(), out result))
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        /// <summary>
        /// 转换为decimal类型
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="defaultValue">返回的默认值</param>
        /// <returns></returns>
        public static double ConvertToDouble(object obj, double defaultValue)
        {
            double result = defaultValue;
            if (obj != null && obj != DBNull.Value)
            {
                if (!double.TryParse(obj.ToString().Trim(), out result))
                {
                    result = 0d;
                }
            }
            return result;
        }

        /// <summary>
        /// 将日期和时间的指定字符串表示形式转换为其等效的System.DateTime。
        /// </summary>
        /// <param name="strDateTime">日期和时间的指定字符串表示形式</param>
        /// <returns>日期时间</returns>
        public static DateTime? ParseDateTime(string strDateTime)
        {
            DateTime retDateTime;
            if (!String.IsNullOrEmpty(strDateTime))
                strDateTime = strDateTime.Trim();

            if (string.IsNullOrEmpty(strDateTime))
                return null;

            if (!DateTime.TryParse(strDateTime, out retDateTime))
            {
                CultureInfo zhCN = new CultureInfo("zh-CN");
                string[] formats = { "yyyyMMdd", "yyyyMMddHH", "yyyyMMddHHmm", "yyyyMMddHHmmss" };
                if (!DateTime.TryParseExact(strDateTime, formats, zhCN, DateTimeStyles.None, out retDateTime))
                {
                    return null;
                }
            }

            return retDateTime;
        }

        /// <summary>
        /// 判断是否为datetime型
        /// </summary>
        /// <param name="date"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        public static bool IsDate(string date, string dateFormat = "")
        {
            try
            {
                DateTime dtResult = new DateTime();
                bool bResult = false;
                // 尝试转换不带格式的日期型
                if(!string.IsNullOrWhiteSpace(dateFormat))
                {
                    bResult = DateTime.TryParse(date, out dtResult);
                }
                // 尝试转换带格式的日期型
                else
                {
                    bResult = DateTime.TryParseExact(date, dateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out dtResult);
                }
                
                return bResult;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// string型转换为bool型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的bool类型结果</returns>
        public static bool ConvertToBool(object Expression, bool defValue)
        {
            if (Expression != null && Expression != DBNull.Value)
            {
                if (string.Compare(Expression.ToString(), "true", true) == 0)
                {
                    return true;
                }
                else if (string.Compare(Expression.ToString(), "false", true) == 0)
                {
                    return false;
                }
            }
            return defValue;
        }

        /// <summary>
        /// string型转换为float型
        /// </summary>
        /// <param name="strValue">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static float ConvertToFloat(string strValue, float defValue)
        {
            if ((strValue == null) || (strValue.Length > 10))
            {
                return defValue;
            }

            float intValue = defValue;
            if (strValue != null)
            {
                bool IsFloat = Regex.IsMatch(strValue, @"^([-]|[0-9])[0-9]*(\.\w*)?$");
                if (IsFloat)
                {
                    float.TryParse(strValue, out intValue);
                }
            }
            return intValue;
        }

        /// <summary>
        /// 解析是否跨天--只支持跨一天的  国内航线没有跨两天的
        /// </summary>
        /// <param name="flyOffDate">起飞时间</param>
        /// <param name="arrDate">抵达时间</param>
        /// <returns></returns>
        public static int GetInterDay(DateTime flyOffDate, DateTime arrDate)
        {
            if (flyOffDate.AddDays(1).ToString("yyyy-MM-dd") == arrDate.ToString("yyyy-MM-dd"))
            {
                return 1;
            }
            return 0;
        }


        #region 全角转半角
        /// <summary>
        /// 全角转半角
        /// </summary>
        /// <param name="input">需要处理字符串</param>
        /// <returns></returns>
        /// 全角空格为12288，半角空格为32
        /// 其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        public static String ToDBC(String input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new String(c);
        }
        #endregion

        #region 过滤掉特殊字符
        private static string[] _invalidSqlRegexs = new string[] { Regex.Escape("/*[]"), Regex.Escape(@"*/[]"), "--", "'", "declare", "select", "into", "insert", "update", "delete", "drop", "create", "exec", "master" };

        public static string NoInvalidSqlString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                goto end;
            }

            foreach (string regex in _invalidSqlRegexs)
            {
                if (string.IsNullOrEmpty(regex))
                {
                    continue;
                }
                text = Regex.Replace(text, regex, "", RegexOptions.IgnoreCase);
            }

        end: return text;
        }


        #endregion

        #region 过滤Html标签
        /// <summary>
        /// 过滤Html标签
        /// </summary>
        /// <param name="Htmlstring"></param>
        /// <returns></returns>
        public static string NoHTML(string Htmlstring)
        {
            if (string.IsNullOrEmpty(Htmlstring))
                return string.Empty;
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "",
              RegexOptions.IgnoreCase);
            //删除HTML
            Htmlstring = Regex.Replace(Htmlstring, @"&mdash;", "",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<style[\s\S]+</style *>", "",
             RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9",
              RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "",
              RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            //Htmlstring = HttpContext.Current.Server.HtmlEncode(Htmlstring).Trim();
            return Htmlstring;
        }
        #endregion

        #region 通过日期获取星期几
        /// <summary>
        /// 通过日期获取星期几
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public static string GetDayOfWeekByDate(string Date)
        {
            DateTime time = ConvertToDateTime(Date, DateTime.Now);
            return time.ToLocalTime().ToString("dddd");
        }

        /// <summary>
        /// 获取当前周几
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public static string GetDayOfWeekzhouByDate(string Date)
        {
            DateTime time = ConvertToDateTime(Date, DateTime.Now);
            return time.ToLocalTime().ToString("ddd");    
        }
        #endregion

        #region 将日期字符串格式化-去掉秒
        /// <summary>
        /// 2014-01-01 12:12:12-->2014-01-01 12:12
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string RemoveDatetimeSeconds(string date)
        {
            string[] arr = null;
            bool ifSuccess = false;
            string dateStr = string.Empty;
            string hourStr = string.Empty;
            if (!string.IsNullOrEmpty(date))
            {
                ifSuccess = true;
            }
            if (ifSuccess)
            {
                arr = date.Split(' ');
            }
            if (arr != null && arr.Length > 1)
            {
                dateStr = arr[0];
                hourStr = arr[1];
            }
            arr = null;
            if (!string.IsNullOrEmpty(hourStr))
            {
                arr = hourStr.Split(':');
            }
            if (arr != null && arr.Length > 2)
            {
                hourStr = string.Format("{0}:{1}", arr[0], arr[1]);
            }

            if (!string.IsNullOrEmpty(dateStr) && !string.IsNullOrEmpty(hourStr))
            {
                date = string.Format("{0} {1}", dateStr, hourStr);
            }
            return date;
        }
        #endregion

        #region 转换两个值为分数
        /// <summary>
        /// 转换两个值为分数
        /// </summary>
        /// <param name="firstNumber"></param>
        /// <param name="secondNumber"></param>
        /// <returns></returns>
        public static string ConverFraction(int firstNumber, int secondNumber)
        {
            int maxy = MaxY(firstNumber, secondNumber);
            if (maxy == 0 || maxy == 1)
            {
                return firstNumber + "/" + secondNumber;
            }
            int minfirst = firstNumber / maxy;
            int minsecond = secondNumber / maxy;
            return minfirst + "/" + minsecond;
        }
        #endregion

        #region 转换两个值为分数  大约值
        /// <summary>
        /// 转换两个值为分数
        /// </summary>
        /// <param name="firstNumber"></param>
        /// <param name="secondNumber"></param>
        /// <returns></returns>
        public static string ConverFractionNew(int firstNumber, int secondNumber)
        {
            if (firstNumber <= 0 || secondNumber <= 0)
            {
                return "0";
            }
            string first = firstNumber.ToString();
            string second = secondNumber.ToString();
            int digitDiff = second.Length - first.Length;
            int f = ConvertToInt(first.Substring(0, 1), 0);
            int s = ConvertToInt(second.Substring(0, digitDiff + 1), 0);
            if (digitDiff > 0 && s / f >= 2)
            {
                s = s / f;
                f = 1;
            }
            if (s % f == 0 && s / f == 1)
            {
                return "1";
            }
            return ConverFraction(f, s);
        }
        #endregion

        #region 求最大公约数的函数
        /// <summary>
        /// 求最大公约数的函数
        /// </summary>
        /// <param name="firstNumber"></param>
        /// <param name="secondNumber"></param>
        /// <returns></returns>
        private static int MaxY(int firstNumber, int secondNumber)   //
        {
            if (firstNumber == 0 || secondNumber == 0)
            {
                return 0;
            }
            int max = Max(firstNumber, secondNumber);
            int min = Min(firstNumber, secondNumber);
            int r = max % min;
            if (r == 0)
            {
                return min;
            }
            else
            {
                while (r != 0)
                {
                    max = min;
                    min = r;
                    r = max % min;
                }
                return min;
            }

        }
        #endregion

        #region 求两个数的最大值
        /// <summary>
        /// 求两个数的最大值
        /// </summary>
        /// <param name="firstNumber"></param>
        /// <param name="secondNumber"></param>
        /// <returns></returns>
        private static int Max(int firstNumber, int secondNumber)       //
        {
            if (firstNumber > secondNumber)
            {
                return firstNumber;
            }
            return secondNumber;
        }
        #endregion

        #region 求两个数的最小值
        /// <summary>
        /// 求两个数的最小值
        /// </summary>
        /// <param name="firstNumber"></param>
        /// <param name="secondNumber"></param>
        /// <returns></returns>
        private static int Min(int firstNumber, int secondNumber)      //
        {
            if (firstNumber > secondNumber)
            {
                return secondNumber;
            }
            return firstNumber;
        }
        #endregion

        #region 进行base64编码
        /// <summary>
        /// 进行base64编码
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertToBase64(string obj)
        {
            string result = "";
            try
            {
                byte[] bytes = Encoding.Default.GetBytes(obj);
                result = Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        #endregion

        #region 进行base64解码
        /// <summary>
        /// 进行base64解码
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertFromBase64(string obj)
        {
            string result = "";
            try
            {
                byte[] outputb = Convert.FromBase64String(obj);
                result = Encoding.Default.GetString(outputb);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        #endregion

        #region 计算年龄
        /// <summary>
        /// 计算年龄
        /// </summary>
        /// <param name="cardNo">身份证号码</param>
        /// <returns></returns>
        public static int CheckAge(string cardNo, DateTime flyoffdate, int type, DateTime birth)
        {
            if (type == 0 && cardNo.Length < 18)
            {
                return 0;
            }
            int yy = type == 0 ? ConvertToInt(cardNo.Substring(6, 4), 0) : birth.Year;
            int mm = type == 0 ? ConvertToInt(cardNo.Substring(10, 2), 0) : birth.Month;
            int dd = type == 0 ? ConvertToInt(cardNo.Substring(12, 2), 0) : birth.Day;
            int age = 0;
            bool isError = false;
            if ((mm < 1) || (mm > 12) || (dd < 1) || (dd > 31) || (yy < 1) || (mm == 0) || (dd == 0) || (yy == 0))
            {
                isError = true;
            }
            else if (((mm == 4) || (mm == 6) || (mm == 9) || (mm == 11)) && (dd > 30))
            {
                isError = true;
            }
            else if (mm == 2)
            {
                if (dd > 29)
                {
                    isError = true;
                }
                else if ((dd > 28) && (!CheckYear(yy)))
                {
                    isError = true;
                }
            }
            else if ((yy > 9999) || (yy < 0))
            {
                isError = true;
            }
            if (!isError)
            {
                int gdate = flyoffdate.Day;
                int gmonth = flyoffdate.Month;
                int gyear = flyoffdate.Year;
                age = gyear - yy;
                if (mm == gmonth && dd > gdate || mm > gmonth)
                {
                    age = age - 1;
                }
            }
            return age;
        }

        public static bool CheckYear(int yy)
        {
            if (((yy % 4) == 0) && ((yy % 100) != 0) || ((yy % 400) == 0))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }
        #endregion

        #region 数组排序（冒泡排序法）
        /// <summary>
        /// 数组排序（冒泡排序法）
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string[] BubbleSort(string[] r)
        {
            int i, j; //交换标志 
            string temp;
            bool exchange;

            for (i = 0; i < r.Length; i++) //最多做R.Length-1趟排序 
            {
                exchange = false; //本趟排序开始前，交换标志应为假

                for (j = r.Length - 2; j >= i; j--)
                {
                    if (System.String.CompareOrdinal(r[j + 1], r[j]) < 0) //交换条件
                    {
                        temp = r[j + 1];
                        r[j + 1] = r[j];
                        r[j] = temp;

                        exchange = true; //发生了交换，故将交换标志置为真 
                    }
                }

                if (!exchange) //本趟排序未发生交换，提前终止算法 
                {
                    break;
                }
            }
            return r;
        }
        #endregion

        #region  获取数组加密值
        /// <summary>
        /// 获取数组加密值
        /// </summary>
        /// <param name="Sortedstr"></param>
        /// <returns></returns>
        public static string GetMD5ByArray(string[] Sortedstr, string key, string _input_charset)
        {
            //构造待md5摘要字符串
            StringBuilder prestr = new StringBuilder();
            for (int i = 0; i < Sortedstr.Length; i++)
            {
                if (i == Sortedstr.Length - 1)
                {
                    prestr.Append(Sortedstr[i]);
                }
                else
                {
                    prestr.Append(Sortedstr[i] + "&");
                }
            }
            prestr.Append(key);
            return GetMD5(prestr.ToString(), _input_charset);
        }
        #endregion

        #region MD5加密
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="s"></param>
        /// <param name="_input_charset"></param>
        /// <returns></returns>
        public static string GetMD5(string s, string _input_charset)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(_input_charset).GetBytes(s));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }
        #endregion

        #region xml 转化成 dataset
        /// <summary>
        /// xml 转化成 dataset
        /// </summary>
        /// <param name="xmlData">xml数据</param>
        /// <returns></returns>
        public static DataSet ConvertXMLToDataSet(string xmlData)
        {
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
        #endregion

        #region 数字验证
        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="strNum">待测试的字符串</param>
        /// <returns>是则返回true,否则返回false</returns>
        public static bool IsNumber(string strNum)
        {
            if (strNum == null)
                return false;
            return Regex.IsMatch(strNum.Trim(), "^(0|[1-9][0-9]*)$");
        }
        #endregion

        /// <summary>
        /// 泛型类转换
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="value">被转换的类对象</param>
        /// <returns></returns>
        public static T ConverToTEntity<T>(object value)
        {
            Type t = typeof(T);
            PropertyInfo[] t_propinfos = t.GetProperties();
            T obj = (T)t.Assembly.CreateInstance(t.FullName);
            if (value != null)
            {
                Type val_t = value.GetType();

                PropertyInfo[] val_t_propinfos = val_t.GetProperties();

                foreach (PropertyInfo vp in val_t_propinfos)
                {
                    PropertyInfo tp = t_propinfos.FirstOrDefault(m => m.Name == vp.Name);
                    if (tp != null)
                    {
                        object val = vp.GetValue(value, null);
                        tp.SetValue(obj, val, null);
                    }
                }
            }
            return obj;
        }
    }
}

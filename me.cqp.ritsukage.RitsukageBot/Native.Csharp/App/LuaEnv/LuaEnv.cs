﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Linq;
using Native.Csharp.Sdk.Cqp.Enum;
using NLua;

namespace Native.Csharp.App.LuaEnv {
    class LuaEnv {
        private static volatile Lua luaField;
        private static readonly object luaLock = new object();

        public static Lua Lua {
            get {
                if (luaField == null) {
                    lock (luaLock) {
                        if (luaField == null) {
                            luaField = new Lua();
                            luaField.State.Encoding = Encoding.UTF8;
                            luaField.LoadCLRPackage();
                            luaField["handled"] = false; //处理标志
                            Initial(luaField);
                        }
                    }
                }
                return luaField;
            }
        }

        [LuaAPIFunction("cqSetGroupSpecialTitle")]
        public static int SetGroupSpecialTitle(long groupId, long qqId, string specialTitle, int time) {
            TimeSpan span = new TimeSpan(time / 60 / 60 / 24, time / 60 / 60 % 60, time / 60 % 60, time % 60);
            return Common.CqApi.SetGroupSpecialTitle(groupId, qqId, specialTitle, span);
        }

        [LuaAPIFunction("cqSetGroupAnonymousBanSpeak")]
        public static int SetGroupAnonymousBanSpeak(long groupId, string anonymous, int time) {
            TimeSpan span = new TimeSpan(time / 60 / 60 / 24, time / 60 / 60 % 60, time / 60 % 60, time % 60);
            return Common.CqApi.SetGroupAnonymousBanSpeak(groupId, anonymous, span);
        }

        [LuaAPIFunction("cqSetGroupBanSpeak")]
        public static int SetGroupBanSpeak(long groupId, long qqId, int time) {
            TimeSpan span = new TimeSpan(time / 60 / 60 / 24, time / 60 / 60 % 60, time / 60 % 60, time % 60);
            return Common.CqApi.SetGroupBanSpeak(groupId, qqId, span);
        }

        /// <summary>
        /// 初始化lua对象
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        public static void Initial(Lua lua) {
            //API查询遍历的类型列表
            //（可改写为从程序集检索，暂不修改）
            List<Type> searchTypes = new List<Type>
            {
                typeof(LuaApi), 
                typeof(LuaEnv), 
                typeof(XmlApi), 
                typeof(TcpServer), 
                typeof(Tools) 
            };
            foreach(Type t in searchTypes)
            {
                //类型中查找所有静态方法
                MethodInfo[] mis = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach(MethodInfo mi in mis)
                {
                    //获取特性
                    LuaAPIFunctionAttribute lapiattr = mi.GetCustomAttribute<LuaAPIFunctionAttribute>();
                    if (lapiattr != null)
                    {
                        //获取自定义名称
                        string s = lapiattr.Name;
                        if (!string.IsNullOrEmpty(s))
                        {
                            lua.RegisterFunction(s, null, mi);
                        }
                        else
                        {
                            lua.RegisterFunction(mi.Name, null, mi);
                        }
                    }
                }
            }

            #region oldRegister

            /*
            ///////////////
            //酷q类的接口//
            //////////////
            lua.RegisterFunction("cqCode_At", null, typeof(LuaApi).GetMethod("CqCode_At"));
            //获取酷Q "At某人" 代码
            lua.RegisterFunction("cqCqCode_Emoji", null, typeof(LuaApi).GetMethod("CqCode_Emoji"));
            //获取酷Q "emoji表情" 代码
            lua.RegisterFunction("cqCqCode_Face", null, typeof(LuaApi).GetMethod("CqCode_Face"));
            //获取酷Q "表情" 代码
            lua.RegisterFunction("cqCqCode_Shake", null, typeof(LuaApi).GetMethod("CqCode_Shake"));
            //获取酷Q "窗口抖动" 代码
            lua.RegisterFunction("cqCqCode_Trope", null, typeof(LuaApi).GetMethod("CqCode_Trope"));
            //获取字符串的转义形式
            lua.RegisterFunction("cqCqCode_UnTrope", null, typeof(LuaApi).GetMethod("CqCode_UnTrope"));
            //获取字符串的非转义形式
            lua.RegisterFunction("cqCqCode_ShareLink", null, typeof(LuaApi).GetMethod("CqCode_ShareLink"));
            //获取酷Q "链接分享" 代码
            lua.RegisterFunction("cqCqCode_ShareCard", null, typeof(LuaApi).GetMethod("CqCode_ShareCard"));
            //获取酷Q "名片分享" 代码
            lua.RegisterFunction("cqCqCode_ShareGPS", null, typeof(LuaApi).GetMethod("CqCode_ShareGPS"));
            //获取酷Q "位置分享" 代码
            lua.RegisterFunction("cqCqCode_Anonymous", null, typeof(LuaApi).GetMethod("CqCode_Anonymous"));
            //获取酷Q "匿名" 代码
            lua.RegisterFunction("cqCqCode_Image", null, typeof(LuaApi).GetMethod("CqCode_Image"));
            //获取酷Q "图片" 代码
            lua.RegisterFunction("cqCqCode_Music", null, typeof(LuaApi).GetMethod("CqCode_Music"));
            //获取酷Q "音乐" 代码
            lua.RegisterFunction("cqCqCode_MusciDIY", null, typeof(LuaApi).GetMethod("CqCode_MusciDIY"));
            //获取酷Q "音乐自定义" 代码
            lua.RegisterFunction("cqCqCode_Record", null, typeof(LuaApi).GetMethod("CqCode_Record"));
            //获取酷Q "语音" 代码
            lua.RegisterFunction("cqSendGroupMessage", null, typeof(LuaApi).GetMethod("SendGroupMessage"));
            //发送群消息
            lua.RegisterFunction("cqSendPrivateMessage", null, typeof(LuaApi).GetMethod("SendPrivateMessage"));
            //发送私聊消息
            lua.RegisterFunction("cqSendDiscussMessage", null, typeof(LuaApi).GetMethod("SendDiscussMessage"));
            //发送讨论组消息
            lua.RegisterFunction("cqSendPraise", null, typeof(LuaApi).GetMethod("SendPraise"));
            //发送赞
            lua.RegisterFunction("cqRepealMessage", null, typeof(LuaApi).GetMethod("RepealMessage"));
            //撤回消息
            lua.RegisterFunction("cqGetLoginQQ", null, typeof(LuaApi).GetMethod("GetLoginQQ"));
            //取登录QQ
            lua.RegisterFunction("cqGetLoginNick", null, typeof(LuaApi).GetMethod("GetLoginNick"));
            //获取当前登录QQ的昵称
            lua.RegisterFunction("cqAppDirectory", null, typeof(LuaApi).GetMethod("GetAppDirectory"));
            //取应用目录
            lua.RegisterFunction("cqGetQQInfo", null, typeof(LuaApi).GetMethod("GetQQInfo"));
            //获取QQ账户信息
            lua.RegisterFunction("cqGetMemberInfo", null, typeof(LuaApi).GetMethod("GetMemberInfo"));
            //获取群成员信息
            lua.RegisterFunction("cqGetMemberList", null, typeof(LuaApi).GetMethod("GetMemberList"));
            //获取群成员列表
            lua.RegisterFunction("cqGetGroupList", null, typeof(LuaApi).GetMethod("GetGroupList"));
            //获取群列表
            lua.RegisterFunction("cqAddLoger", null, typeof(LuaApi).GetMethod("AddLoger"));
            //添加日志
            lua.RegisterFunction("cqAddFatalError", null, typeof(LuaApi).GetMethod("AddFatalError"));
            //添加致命错误提示
            lua.RegisterFunction("cqSetGroupWholeBanSpeak", null, typeof(LuaApi).GetMethod("SetGroupWholeBanSpeak"));
            //置全群禁言
            lua.RegisterFunction("cqSetGroupMemberNewCard", null, typeof(LuaApi).GetMethod("SetGroupMemberNewCard"));
            //置群成员名片
            lua.RegisterFunction("cqSetGroupManager", null, typeof(LuaApi).GetMethod("SetGroupManager"));
            //置群管理员
            lua.RegisterFunction("cqSetAnonymousStatus", null, typeof(LuaApi).GetMethod("SetAnonymousStatus"));
            //置群匿名设置
            lua.RegisterFunction("cqSetGroupMemberRemove", null, typeof(LuaApi).GetMethod("SetGroupMemberRemove"));
            //置群员移除
            lua.RegisterFunction("cqSetDiscussExit", null, typeof(LuaApi).GetMethod("SetDiscussExit"));
            //置讨论组退出
            lua.RegisterFunction("cqSetGroupSpecialTitle", null, typeof(LuaEnv).GetMethod("SetGroupSpecialTitle"));
            //置群成员专属头衔
            lua.RegisterFunction(
                "cqSetGroupAnonymousBanSpeak",
                null,
                typeof(LuaEnv).GetMethod("SetGroupAnonymousBanSpeak")
            );
            //置匿名群员禁言
            lua.RegisterFunction("cqSetGroupBanSpeak", null, typeof(LuaEnv).GetMethod("SetGroupBanSpeak"));
            //置群员禁言
            lua.RegisterFunction("cqSetFriendAddRequest", null, typeof(LuaApi).GetMethod("SetFriendAddRequest"));
            //置好友添加请求
            lua.RegisterFunction("cqSetGroupAddRequest", null, typeof(LuaApi).GetMethod("SetGroupAddRequest"));
            //置群添加请求

            /////////////
            //工具类接口//
            /////////////
            lua.RegisterFunction("apiGetPath", null, typeof(LuaApi).GetMethod("GetPath"));
            //获取程序运行目录
            lua.RegisterFunction("apiGetBitmap", null, typeof(LuaApi).GetMethod("GetBitmap"));
            //获取图片对象
            lua.RegisterFunction("apiPutText", null, typeof(LuaApi).GetMethod("PutText"));
            //摆放文字
            lua.RegisterFunction("apiPutBlock", null, typeof(LuaApi).GetMethod("PutBlock"));
            //填充矩形
            lua.RegisterFunction("apiSetImage", null, typeof(LuaApi).GetMethod("SetImage"));
            //摆放图片
            lua.RegisterFunction("apiGetDir", null, typeof(LuaApi).GetMethod("GetDir"));
            //保存并获取图片路径

            lua.RegisterFunction("apiGetImagePath", null, typeof(LuaApi).GetMethod("GetImagePath"));
            //获取qq消息中图片的网址

            lua.RegisterFunction("apiHttpDownload", null, typeof(LuaApi).GetMethod("HttpDownload"));
            //下载文件
            lua.RegisterFunction("apiHttpGet", null, typeof(LuaApi).GetMethod("HttpGet"));
            //GET 请求与获取结果
            lua.RegisterFunction("apiHttpPost", null, typeof(LuaApi).GetMethod("HttpPost"));

            lua.RegisterFunction("OHTTPCreateRequest", null, typeof(LuaApi).GetMethod("OHTTPCreateRequest"));
            lua.RegisterFunction("OHTTPSetMethod", null, typeof(LuaApi).GetMethod("OHTTPSetMethod"));
            lua.RegisterFunction("OHTTPSetTimeout", null, typeof(LuaApi).GetMethod("OHTTPSetTimeout"));
            lua.RegisterFunction("OHTTPContentType", null, typeof(LuaApi).GetMethod("OHTTPContentType"));
            lua.RegisterFunction("OHTTPUserAgent", null, typeof(LuaApi).GetMethod("OHTTPUserAgent"));
            lua.RegisterFunction("OHTTPSetCookie", null, typeof(LuaApi).GetMethod("OHTTPSetCookie"));
            lua.RegisterFunction("OHTTPSetReferer", null, typeof(LuaApi).GetMethod("OHTTPSetReferer"));
            lua.RegisterFunction("OHTTPGetResponse", null, typeof(LuaApi).GetMethod("OHTTPGetResponse"));

            lua.RegisterFunction("Execute", null, typeof(LuaApi).GetMethod("Execute"));

            lua.RegisterFunction("SetMySQL", null, typeof(LuaApi).GetMethod("SetMySQL"));
            lua.RegisterFunction("ConnectMySQL", null, typeof(LuaApi).GetMethod("ConnectMySQL"));
            lua.RegisterFunction("DisconnectMySQL", null, typeof(LuaApi).GetMethod("DisconnectMySQL"));
            lua.RegisterFunction("MySQLDoCommand", null, typeof(LuaApi).GetMethod("MySQLDoCommand"));
            lua.RegisterFunction("GetMySQLLastDataReader", null, typeof(LuaApi).GetMethod("GetMySQLLastDataReader"));
            
            //lua.RegisterFunction("MySQLDataReaderNextLine", null, typeof(LuaApi).GetMethod("MySQLDataReaderNextLine"));
            //lua.RegisterFunction("MySQLDataReaderCheckEmptyByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderCheckEmptyByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetNameByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetNameByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetBooleanByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetBooleanByName"));
            //lua.RegisterFunction("MySQLDataReaderGetBooleanByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetBooleanByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetByteByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetByteByName"));
            //lua.RegisterFunction("MySQLDataReaderGetByteByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetByteByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetCharByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetCharByName"));
            //lua.RegisterFunction("MySQLDataReaderGetCharByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetCharByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetInt16ByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetInt16ByName"));
            //lua.RegisterFunction("MySQLDataReaderGetInt16ByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetInt16ByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetInt32ByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetInt32ByName"));
            //lua.RegisterFunction("MySQLDataReaderGetInt32ByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetInt32ByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetInt64ByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetInt64ByName"));
            //lua.RegisterFunction("MySQLDataReaderGetInt64ByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetInt64ByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetUInt16ByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetUInt16ByName"));
            //lua.RegisterFunction("MySQLDataReaderGetUInt16ByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetUInt16ByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetUInt32ByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetUInt32ByName"));
            //lua.RegisterFunction("MySQLDataReaderGetUInt32ByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetUInt32ByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetUInt64ByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetUInt64ByName"));
            //lua.RegisterFunction("MySQLDataReaderGetUInt64ByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetUInt64ByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetDateTimeByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetDateTimeByName"));
            //lua.RegisterFunction("MySQLDataReaderGetDateTimeByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetDateTimeByColumn"));
            //lua.RegisterFunction("MySQLDataReaderGetStringByName", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetStringByName"));
            //lua.RegisterFunction("MySQLDataReaderGetStringByColumn", null, typeof(LuaApi).GetMethod("MySQLDataReaderGetStringByColumn"));
            

            //POST 请求与获取结果
            lua.RegisterFunction("apiBase64File", null, typeof(LuaApi).GetMethod("Base64File"));
            //获取在线文件的base64结果
            lua.RegisterFunction("apiGetPictureWidth", null, typeof(LuaApi).GetMethod("GetPictureWidth"));
            //获取在线文件的base64结果
            lua.RegisterFunction("apiGetPictureHeight", null, typeof(LuaApi).GetMethod("GetPictureHeight"));
            //获取在线文件的base64结果
            lua.RegisterFunction("apiSetVar", null, typeof(LuaApi).GetMethod("SetVar"));
            //设置某值存入ram
            lua.RegisterFunction("apiGetVar", null, typeof(LuaApi).GetMethod("GetVar"));
            //取出某缓存的值
            lua.RegisterFunction("apiGetAsciiHex", null, typeof(LuaApi).GetMethod("GetAsciiHex"));
            //获取字符串ascii编码的hex串
            lua.RegisterFunction("apiSetTimerScriptWait", null, typeof(LuaApi).GetMethod("SetTimerScriptWait"));
            //设置定时脚本运行间隔时间

            lua.RegisterFunction("apiGetHardDiskFreeSpace", null, typeof(Tools).GetMethod("GetHardDiskFreeSpace"));
            //获取指定驱动器的剩余空间总大小(单位为MB)
            lua.RegisterFunction("apiMD5Encrypt", null, typeof(Tools).GetMethod("MD5Encrypt"));
            //计算MD5

            lua.RegisterFunction("apiTcpSend", null, typeof(TcpServer).GetMethod("Send"));
            //发送tcp广播数据

            lua.RegisterFunction("apiSandBox", null, typeof(LuaEnv).GetMethod("RunSandBox"));
            //沙盒环境

            ///////////////
            //XML操作接口//
            //////////////
            lua.RegisterFunction("apiXmlReplayGet", null, typeof(XmlApi).GetMethod("replay_get"));
            //随机获取一条结果
            lua.RegisterFunction("apiXmlListGet", null, typeof(XmlApi).GetMethod("list_get"));
            //获取所有回复的列表
            lua.RegisterFunction("apiXmlDelete", null, typeof(XmlApi).GetMethod("del"));
            //删除所有匹配的条目
            lua.RegisterFunction("apiXmlRemove", null, typeof(XmlApi).GetMethod("remove"));
            //删除完全匹配的第一个条目
            lua.RegisterFunction("apiXmlInsert", null, typeof(XmlApi).GetMethod("insert"));
            //插入一个词条
            lua.RegisterFunction("apiXmlSet", null, typeof(XmlApi).GetMethod("set"));
            //更改某条的值
            lua.RegisterFunction("apiXmlGet", null, typeof(XmlApi).GetMethod("xml_get"));
            //获取某条的结果
            lua.RegisterFunction("apiXmlRow", null, typeof(XmlApi).GetMethod("xml_row"));
            //按结果查源头（反查）
            */

            #endregion

            lua.DoFile(Common.AppDirectory + "lua/require/head.lua");


            ////// sand box Function (from SandBox Method)
//            try {
//                lua.RegisterFunction("apiGetPath", null, typeof(LuaApi).GetMethod("GetPath"));
//                //获取程序运行目录
//                lua.RegisterFunction("apiGetAsciiHex", null, typeof(LuaApi).GetMethod("GetAsciiHex"));
//            } catch (Exception e) {
//                Common.CqApi.AddLoger(LogerLevel.Error, "lua注册SandBox错误", e.ToString());
//            }
        }


        /// <summary>
        /// 运行lua文件
        /// </summary>
        /// <param name="code">提前运行的代码</param>
        /// <param name="file">文件路径（app/xxx.xxx.xx/lua/开头）</param>
        public static bool RunLua(string code, string file, ArrayList args = null) {
            //还没下载lua脚本，先不响应消息
            if (!File.Exists(Common.AppDirectory + "lua/require/head.lua")) return false;

            lock (luaLock) {
                var lua = Lua;
                try {
                    //maybe.... only once? by yys
//                    lua.State.Encoding = Encoding.UTF8;
//                    lua.LoadCLRPackage();
//                    lua["handled"] = false;//处理标志
//                    Initial(lua);
                    if (args != null)
                        for (int i = 0; i < args.Count; i += 2) {
                            lua[(string) args[i]] = args[i + 1];
                        }
                    lua.DoString(code);
                    if (file != "") lua.DoFile(Common.AppDirectory + "lua/" + file);
                    return (bool) lua["handled"];
                } catch (Exception e) {
                    Common.CqApi.AddLoger(LogerLevel.Error, "lua脚本错误", e.ToString());
                    return false;
                }
            }
        }

        /// <summary>
        /// 在沙盒中运行代码，仅允许安全地运行
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [LuaAPIFunction("apiSandBox")]
        public static string RunSandBox(string code) {
            using (var lua = new Lua()) {
                lock (luaLock) {
                    try {
                        //函数干脆一起注册了 部分也在new的时候操作了 by:yys canceled by:yys
                        lua.State.Encoding = Encoding.UTF8;
                        lua.RegisterFunction("apiGetPath", null, typeof(LuaApi).GetMethod("GetPath"));
                        //获取程序运行目录
                        lua.RegisterFunction("apiGetAsciiHex", null, typeof(LuaApi).GetMethod("GetAsciiHex"));
                        //获取字符串ascii编码的hex串
                        lua["lua_run_result_var"] = ""; //返回值所在的变量
                        lua.DoFile(Common.AppDirectory + "lua/require/sandbox/head.lua");
                        lua.DoString(code);
                        return lua["lua_run_result_var"].ToString();
                    } catch (Exception e) {
                        Common.CqApi.AddLoger(LogerLevel.Error, "沙盒lua脚本错误", e.ToString());
                        return "运行错误：" + e;
                    }
                }
            }
        }
    }
}
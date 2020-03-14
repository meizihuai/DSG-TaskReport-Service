using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service
{
    class EmailHelper
    {
        public static NormalResponse SendMail(string EmailAddress, string Subject, string Content, Hashtable AttachFile = null)
        {
            try
            {
                int i;
                // SMTP客户端  
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("SMTP.139.COM");
                // smtp.Host = "smtp.163.com"       'SMTP服务器名称  
                // 发件人邮箱身份验证凭证。 参数分别为 发件邮箱登录名和密码  
                smtp.Credentials = new System.Net.NetworkCredential("18319087172@139.com", "QQlove12345");
                // 创建邮件  
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                // 主题编码  
                mail.SubjectEncoding = Encoding.UTF8;
                // 正文编码  
                mail.BodyEncoding = Encoding.UTF8;
                // 邮件优先级  
                mail.Priority = System.Net.Mail.MailPriority.Normal;
                // 以HTML格式发送邮件,为false则发送纯文本邮箱  
                mail.IsBodyHtml = true;
                // 发件人邮箱  
                mail.From = new System.Net.Mail.MailAddress("18319087172@139.com");

                // 添加收件人,如果有多个,可以多次添加  
                List<string> ReceiveAddressList = new List<string>();
                if (!EmailAddress.Contains("."))
                    ReceiveAddressList.Add(EmailAddress);
                else
                    foreach (var sh in EmailAddress.Split(','))
                        ReceiveAddressList.Add(sh);
                if (ReceiveAddressList.Count == 0)
                    return new NormalResponse(false,"没有收件人");
                for (i = 0; i <= ReceiveAddressList.Count - 1; i++)
                    mail.To.Add(ReceiveAddressList[i]);

                // 邮件主题和内容  
                mail.Subject = Subject;
                mail.Body = Content;

                // 定义附件,参数为附件文件名,包含路径,推荐使用绝对路径  
                if (AttachFile != null && AttachFile.Count != 0)
                {
                    foreach (string sKey in AttachFile.Keys)
                    {
                     
                        System.Net.Mail.Attachment objFile = new System.Net.Mail.Attachment(AttachFile[sKey].ToString());
                        // 附件文件名,用于收件人收到附件时显示的名称  
                        objFile.Name = sKey;
                        // 加入附件,可以多次添加  
                        mail.Attachments.Add(objFile);
                    }
                }

                // 发送邮件  
                smtp.Send(mail);
                return new NormalResponse(true, "邮件发送成功");
            }
            catch (Exception ex)
            {
                return new NormalResponse(false, $"邮件发送失败，{ex.ToString()}");
            }
        }

    }
}

using MailboxIntegration.Helpers;
using MailboxIntegration.Models;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MailboxIntegration.Controllers
{
    public class MailController : BaseController
    {
        public async Task<JsonResult> MailInboxSearch(string sharedMailId)
        {
            MailListDetail mailList = new MailListDetail();
            var mailListDetail = (dynamic)null;
            if (!string.IsNullOrEmpty(sharedMailId)){
                mailListDetail = await GraphHelper.OpenSharedMailbox(sharedMailId);
            }
            else
            {
                mailListDetail = await GraphHelper.GetEventsAsync();
            }
          
            mailList = GetMessageList(mailListDetail);

            return Json(new
            {
                view = RenderRazorViewToString(ControllerContext, "MailboxView", mailList.Items.ToList()),isValid = true,itemsCount = mailList.Items.ToList()},JsonRequestBehavior.AllowGet); ;
        }
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var ViewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var ViewContext = new ViewContext(controllerContext, ViewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                ViewResult.View.Render(ViewContext, sw);
                ViewResult.ViewEngine.ReleaseView(controllerContext, ViewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        public MailListDetail GetMessageList(IEnumerable<Message> mailListDetail)
        {
            MailListDetail mailList = new MailListDetail();
            List<AttachmentProperties> attachmentList;
            List<MailListDetailItems> items = new List<MailListDetailItems>();
            if (mailListDetail != null)
            {
                foreach (Message message in mailListDetail)
                {
                    attachmentList = new List<AttachmentProperties>();
                    if (message.Attachments != null)//message.Attachments.Count != 0 && 
                    {

                        foreach (var item in message.Attachments.CurrentPage.ToList())
                        {
                            attachmentList.Add(new AttachmentProperties
                            {
                                AttachmentName = item.Name,
                                AttachmentSize = item.Size,
                                AttachmentType = item.ODataType,
                                AttachmentId = item.Id
                            });
                        }
                    }
                    items.Add(new MailListDetailItems
                    {
                        EmailID = message.From.EmailAddress.Address,
                        Display = message.Subject,
                        Id = message.Id,
                        Message = message.BodyPreview,
                        Properties = attachmentList
                    });
                }
                mailList.Items = items;
            }
            return mailList;
        }

        public async Task<object> DownLoadAttachment(MailListDetailItems mailListDetailItems)
        {
            try
            {
                var attachmentRequest = await GraphHelper.DownloadAttachments(mailListDetailItems.Id);

                if (!System.IO.Directory.Exists(ConfigurationManager.AppSettings["filePath"]))
                {
                    CreateDiretoryIfNotExists(ConfigurationManager.AppSettings["filePath"]);
                }
                var filePath = ConfigurationManager.AppSettings["filePath"] + "/" + mailListDetailItems.EmailID;
                if (!System.IO.Directory.Exists(filePath))
                {
                    CreateDiretoryIfNotExists(filePath);
                }
                foreach (var item in attachmentRequest)
                {
                    var fileAttachment = (FileAttachment)item;
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(filePath, fileAttachment.Name), fileAttachment.ContentBytes);
                }
                var redirectUrl = new System.Web.Mvc.UrlHelper(Request.RequestContext).Action("Index", "Mail", new { });
                return Json(new { Url = redirectUrl, status = "OK" });
            }
            catch (Exception ex)
            {
                Flash(ex.Message, null);
                throw;
            }

        }

        public string CreateDiretoryIfNotExists(string filePath)
        {
            System.IO.Directory.CreateDirectory(filePath);
            return "";
        }

        public PartialViewResult OpenSharedMailbox(string sharedMailId)
        {
            try
            {
                MailListDetail mailList = new MailListDetail();
                var mailListDetail = GraphHelper.OpenSharedMailbox(sharedMailId);
                mailList = GetMessageList(mailListDetail.Result);
                return PartialView("MailBoxView", mailList.Items.ToList());
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
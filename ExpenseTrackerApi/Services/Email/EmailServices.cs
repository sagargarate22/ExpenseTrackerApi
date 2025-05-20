
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using ExpenseTrackerApi.Services.Email.Interface;

namespace ExpenseTrackerApi.Services.Email
{

    public enum EmailType
    {
        Welcome,
        Verify,
        Daily,
        Monthly
    }

    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration _configuration;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly ILogger<EmailServices> _logger;
        private readonly IServiceProvider _provider;
        public EmailServices(IConfiguration configuration, ILogger<EmailServices> logger,
            IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IServiceProvider provider)
        {
            _configuration = configuration;
            _logger = logger;
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _provider = provider;
        }

        public async Task Send(string toEmail, string subject,EmailType emailType,object model)
        {
            try
            {
                var body = await RenderViewToStringAsync(emailType, model);
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpClient = new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"]!))
                {
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["SenderEmail"]!, smtpSettings["SenderName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                if (e.InnerException != null)
                {
                    _logger.LogInformation($"Inner Exception: {e.InnerException.Message}");
                }
            }
        }


        //public async Task<string> RenderViewToStringAsync(EmailType viewName, object model)
        //{
        //    var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = _razorViewEngine.GetView("~/Views/Emails/", $"{viewName}.cshtml", false);
        //        if (viewResult.View == null)
        //            throw new FileNotFoundException($"View {viewName.ToString()} not found");

        //        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        //        {
        //            Model = model
        //        };

        //        var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, new TempDataDictionary(actionContext.HttpContext, _tempDataProvider), sw, new HtmlHelperOptions());
        //        await viewResult.View.RenderAsync(viewContext);
        //        return sw.ToString();
        //    }
        //}

        public async Task<string> RenderViewToStringAsync(EmailType viewName, object model)
        {
            // Create an ActionContext
            var httpContext = new DefaultHttpContext { RequestServices = _provider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                // Search for the view using GetView with the absolute path
                var viewResult = _razorViewEngine.FindView(actionContext, $"Emails/{viewName.ToString()}", false);
                //var viewResult = _razorViewEngine.GetView("~/Views/Emails/", $"{viewName}.cshtml", false);

                if (!viewResult.Success)
                {
                    throw new FileNotFoundException($"View {viewName.ToString()} not found. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");
                }

                // Create ViewDataDictionary and TempDataDictionary
                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
                var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

                // Create the ViewContext
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    tempData,
                    sw,
                    new HtmlHelperOptions()
                );

                // Render the view
                try
                {
                    await viewResult.View.RenderAsync(viewContext);
                }
                catch (Exception ex)
                {
                    // Log the full exception message and stack trace for investigation
                    throw new InvalidOperationException($"Error rendering view: {ex.Message}", ex);
                }

                // Return the rendered string
                return sw.ToString();
            }
        }

    }
}

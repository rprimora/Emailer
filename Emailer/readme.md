# Welcome to Emailer!
![enter image description here](https://i.imgur.com/zprf5Jm.png)

Emailer is a simple service used to generate email contet from razor page and send that content to a recipient via email. This was from personal need of the author to share functionality between projects.

# Setup

Emailer consist of two services which must be called in this order.
 1. RazorViewToStringRenderer
 2. Email

## RazorViewToStringRenderer

This service is responsible for rendering razor view(`*.cshtml`) to string. Service is added in the `ConfigureServices` method in following way:

    services.AddRazorViewToStringRenderer(options => 
    {
    	options.ContentRoot = HostingEnvironment.ContentRootPath;
    	options.EmailsFolder = "Emails";
    });

 - `ContentRoot` - this option is needed so the service knows where to look for views. It is obtainable via `IHostingEnvironment` interface.
 - `EmailsFolder` - this is optional. Default value is `"Emails"` and it is the folder in Views that holds all the email razor pages.

## Email

This service is responsible for sending the previously rendered views to the recipients via email. When adding the service there are two ways to define options needed to create the `SmtpClient`.

### 1. Via options action

Pass an `Action<EmailOptions>` to the method that adds this service:

    services.AddEmail(options => 
    {
    	options.Port = 587;
        options.Host = "smtp.gmail.com";
        options.EnableSsl = true;
        options.Timeout = 10000;
        options.Sender = "yourmail@gmail.com";
        options.Username = "yourmail@gmail.com";
        options.Password = "passssssss";
    });

### 2. Via IConfiguration

For this to work you have to define a configuration section named `SmtpSettings` in the `appsettings.json`:

    "SmtpSettings": {
        "Port": 587,
        "Host": "smtp.gmail.com",
        "EnableSsl": true,
        "Timeout": 10000,
        "Sender": "rprimora@gmail.com",
        "Username": "rprimora@gmail.com",
        "Password": "xtvxsuiheuyiqipc"
      }

> **Tip 1**: Default value of `Timeout` is 10000 and if that suits you you do not have to pass it.
> **Tip 2**: It is not needed to define `Sender` if it's same as `Username`.

The service is than added in following manner:

    services.AddEmail(configuration);

## Using the email service

In order to use the email service you have to obtain it via `IServiceProvider` in following manner.

    IServiceProvider.GetService<IEmail>()

Interface describes two methods:

 - `Task SendEmailAsync<TModel>(TModel model) where TModel: EmailModel;`
 - `Task SendEmailAsync<TModel>(Func<SmtpClient> client, TModel model) where TModel : EmailModel;`

Both methods send email that is defined in the given `TModel`. Second method also accepts a `Func<SmtpClient>` that returns a `SmtpClient` which is used to send email. In this case whatever you defined while adding the Email service is ignored.

## EmailModel

Base model for email defines some basic information that is needed almost always when sending an email:

 - `EmailView` - this is the name of the razor page which represents our email body.
 - `Subject` - email subject.
 - `To` - email address of the recipient.

If the email has no special changing data(e.g. activation link) than this is enough to send the email. If you have need to send some other information in email than you would inherit `EmailModel` and define your information there.

### Example

Define the activation email model by inheriting `EmailModel`:

    public class ActivationEmailModel : EmailModel
    {
    	public string ActivationLink { get; set; }
    }
Create razor page in `Views/Emails` and name it `ActivationEmail`:

    @model ActivationEmailModel
    <div>
    	Thank you for registering with us please activate you account by clicking the link below
    	<a href="@Model.ActivationLink">Activate</a>
    </div>

Send you email by first obtaining the email service:

    var emailService = HttpContext.RequestServices.GetRequiredService<IEmail>()

Now define your model:

    var activationModel = new ActivationEmailModel()
    {
    	EmailView = "ActivationEmail",
    	Subject = "Activate your account",
    	To = "recipient@mail.com",
    	ActivationLink = "yoursite.com/account/activate?id=abc123"
    };
Now send the email:

    await emailService.SendEmailAsync(activationModel);


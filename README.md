
# Welcome to Emailer!
![enter image description here](https://i.imgur.com/zprf5Jm.png)

Emailer is a simple service used to generate email contet from razor page and send that content to a recipient via email. This project was written from personal need of the author to share functionality between projects.

# Setup

Emailer uses ViewRenderer service internally so in order for it to work you have to set up this service first. For details please check [ViewRenderer](https://github.com/rprimora/ViewRenderer) readme.

At the moment the Emailer consist of two implementations:
1. [SmtpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient)
2. [SendGrid](https://sendgrid.com/)

## SmtpClient implementation
This implementation uses [SmtpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient) from [System.Net.Mail](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail) namespace. When adding the service there are two ways to define options needed to create the `SmtpClient` used for sending the email messages.

### 1. Via options action

Pass an `Action<SmtpOptions>` to the method that adds this service:

    services.AddSmtpEmailer(options => 
    {
        options.Port = 587;
        options.Host = "smtp.mail.com";
        options.EnableSsl = true;
        options.Timeout = 10000;
        options.Senders = new Dictionary<string, SenderInformation>() {
	        ["default"] = new SenderInformation(){
		        Email = "john.doe@mail.com",
		        Password = "johndoe01",
		        Name = "John Doe"
	        },
	        ["noreply"] = new SenderInformation(){
		        Email = "noreply@email.com",
		        Pasword = "noreply01",
		        Name = "No reply"
	        }
        }
    });

### 2. Via IConfiguration

For this to work you have to define a configuration section named `SmtpSettings` in the `appsettings.json`:

    "SmtpSettings": {
        "Port": 587,
        "Host": "smtp.mail.com",
        "EnableSsl": true,
        "Timeout": 10000,
        "Senders": {
	        "default": {
		        "Email": "john.doe@mail.com",
		        "Password" = "johndoe01",
		        "Name" = "John Doe"
	        },
	        "noreply": {
		        "Email": "noreply@email.com",
		        "Password": "noreply01",
		        "Name": "No reply"
	        }
        }
      }

> **Note**
> - Default value of `Timeout` is 10000 and if that suits you you do not have to pass it.
> - `Email` and `Password` are credentials used to connect to the Smtp server and `Name` is the display name for the email message. 

The service is than added in following manner:

    services.AddSmtpEmailer(configuration);

## SendGrid implementation
This implementation uses [SendGrid](https://sendgrid.com/docs/for-developers/sending-email/v3-csharp-code-example/). When adding the service there are two ways to define options needed to create the `SendGridClient`.

### 1. Via options action

Pass an `Action<SendGridOptions>` to the method that adds this service:

    services.AddSendGridEmailer(options => 
    {
        options.APIKey = "SG.dOkDQWGrTyWtVhM36W8YDw.PDKhPsqOUnocviXq9YYQRxSYAdUqfgaKyZ011OsT6A";
        options.Senders = new Dictionary<string, SenderInformation>() {
	        ["default"] = new SenderInformation(){
		        Email = "john.doe@mail.com",
		        Name = "John Doe"
	        },
	        ["additional"] = new SenderInformation(){
		        Email = "charles.doe@email.com",
		        Name = "Charles Doe"
	        }
        }
    });

### 2. Via IConfiguration

For this to work you have to define a configuration section named `SmtpSettings` in the `appsettings.json`:

    "SendGridSettings": {
        "APIKey": "SG.dOkDQWGrTyWtVhM36W8YDw.PDKhPsqOUnocviXq9YYQRxSYAdUqfgaKyZ011OsT6A",
        "Senders": {
	        "default": {
		        "Email": "john.doe@mail.com",
		        "Name" = "John Doe"
	        },
	        "additional": {
		        "Email": "charles.doe@email.com",
		        "Name": "Charles Doe"
	        }
        }
      }

The service is than added in following manner:

    services.AddSendGridEmailer(configuration);

## Using the email service

In order to use the email service you have to obtain it via `IServiceProvider` in following manner.

    IServiceProvider.GetService<IEmailer>()

Interface describes two methods:

1. `Task SendEmailAsync<TModel>(TModel model) where TModel: EmailModel;` 
2. `Task SendEmailAsync<TModel>(TModel model, string sender) where TModel : EmailModel;`

First method will send the email as the default sender. It is presumed that the sender with key "default" is defined in any of the previously desribed ways.
Second method takes in the sender key and uses the sender information defined under the given key.

### EmailModel

Base model for email defines some basic information that is needed almost always when sending an email:
 - `EmailView` - this is the name of the razor page which represents our email body. This information is used by [ViewRenderer](https://github.com/rprimora/ViewRenderer) service.
 - `Subject` - email subject.
 - `To` - email address of the recipient.

>**Note**: If the email has no special data(e.g. activation link) than the base model is enough to send the email. If you have the need to send some other information in email than you would inherit `EmailModel` and define your information there.

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

Send you email by first obtaining the email service. In this particular example we are obtaining the service from HttpContext.

    var emailService = HttpContext.RequestServices.GetRequiredService<IEmailer>()

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
This particular method uses the `default` sender and if this method is used than it is presumed that the defualt sender is in fact defined. If we wanted to use another sender, e.g. "noreply" we would call the second overload like so:

    await emailService.SendEmailAsync(activationModel, "noreply");





## Health Checks UI Interface styling and branding

Since version 2.2.34 the UI interface has a new feature to add custom stylesheets that will be served by the UI middleware, allowing to customize most interface sections:

![HealthChecksUIBranding](./images/ui-branding.png)

To customize your styles and branding just register your custom stylesheets in UseHealthChecksUI setup action:


```csharp
 app
  .UseRouting()
  .UseEndpoints(config =>
  {                 
     config.MapHealthChecksUI(setup =>
     {
        setup.AddCustomStylesheet("dotnet.css");
     });
  });

```


We introduced custom properties to change grey scale and main color paillete. Another benefit is the use of BEM like naming convention for css classes.

```
Color paillete

--primaryColor: #5ec297;
--secondaryColor: #349b72;
--darkColor: #000000;
--midDarkColor: #2f313a;
--grayColor: #444444;
--midGrayColor: #ebebeb;
--lightColor: #f6f6f7;
--dangerColor: #d26b4e;
--warningColor: #ff8f30;
--successColor: #3aaa97;

Styles configuration


--bgMain: var(--lightColor);
--bgSecondary: #ffffff;
--fcNegative: #ffffff;
--fcBase: var(--grayColor);
--bgAside: var(--midDarkColor);
--bgMenuActive: var(--darkColor);
--bcMenuActive: var(--primaryColor);
--bcInput: var(--midGrayColor);
--bcInputHover: var(--primaryColor);
--bgInputHover: var(--primaryColor);
--fcInputHover: var(--primaryColor);
--bcTable: var(--midGrayColor);
--bgTable: var(--bgSecondary);
--bgTableSecondary: var(--midGrayColor);
--bgTableButton: var(--bgSecondary);
--bgButton: var(--secondaryColor);
--bgImageDarken: linear-gradient(rgba(0, 0, 0, 0.2), rgba(0, 0, 0, 0.2));
--bgImageLigthen: linear-gradient(
rgba(255, 255, 255, 0.8),
rgba(255, 255, 255, 0.8)
);
--logoImageUrl: url('routetoyourlogo');

```

## Source Sample:


[You can check here the dotnet custom branding sample](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/tree/master/samples/HealthChecks.UI.Branding)
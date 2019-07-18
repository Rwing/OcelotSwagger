# OcelotSwagger

[![NuGet][main-nuget-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/OcelotSwagger/
[main-nuget-badge]: https://img.shields.io/nuget/v/OcelotSwagger.svg?style=flat-square&label=nuget

This library makes ocelot easy to integrate swagger

## Installation

```bash
Install-Package OcelotSwagger
```

## Usage

In method `ConfigureServices`
```csharp
// Load options from code
services.AddOcelotSwagger(c =>
{
    c.Cache.Enabled = true;
    c.SwaggerEndPoints.Add(new SwaggerEndPoint { Name = "Api Name", Url = "/path/swagger.json" });
});
```
Or
```csharp
// Load options from appsettings.json
services.Configure<OcelotSwaggerOptions>(this.configuration.GetSection(nameof(OcelotSwaggerOptions)));
services.AddOcelotSwagger();
```

In method `Configure`
```csharp
app.UseOcelotSwagger();
```

## TODO
* [x] Cache

# OcelotSwagger

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
services.AddOcelotSwagger();

```

In method `Configure`
```csharp
app.UseOcelotSwagger(c =>
{
    c.SwaggerEndPoints.Add(new SwaggerEndPoint { Name = "Api Name", Url = "/path/swagger.json" });
});
```

## TODO
* [x] Cache

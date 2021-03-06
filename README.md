# Health Check Monitor

The health check monitor is a library for Monitor other applications health checks. Reponding to changes in status of those applications, and displaying the status of all the applications.

There is also a second library HealthCheckMonitor.Client to make setting up the client applications easier.

[![Build Status](https://dev.azure.com/hrsid/HRS.HealthMonitor/_apis/build/status/HumanRecognitionSystems.HRS.HealthCheckMonitor?branchName=master)](https://dev.azure.com/hrsid/HRS.HealthMonitor/_build/latest?definitionId=2&branchName=master)
<br>[![NuGet Badge](https://buildstats.info/nuget/HRS.HealthCheckMonitor)](https://www.nuget.org/packages/HRS.HealthCheckMonitor/) - HRS.HealthCheckMonitor
<br>[![NuGet Badge](https://buildstats.info/nuget/HRS.HealthCheckMonitor.Client)](https://www.nuget.org/packages/HRS.HealthCheckMonitor.Client/) - HRS.HealthCheckMonitor.Client

## Monitor Usage
To use the health check monitor in your applcation use the ServiceCollection extension AddHealthCheckMonitor.

```c#
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        //...
        services.AddHealthCheckMonitor();
        //...
    }

    //...
}
```

If you wish to use the Api that generates a json output you will also need to add the IApplicationBuilder extension UseHealthCheckMonitorApi.

```c#
public class Startup
{
    //...

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //...
        services.UseHealthCheckMonitorApi();
        //...
    }

    //...
}
```

## Configuration
The configuration options for the monitor are listed below:
* EvaluationInterval (TimeSpan) - The amount of time between evaluating the health checks. Default is 30 seconds.
* InitialEvaluationDelay (TimeSpan) - The delay before the first evaluation of the health checks. Default is 2 minutes. This is to allow other services to start.
* HealthCheckTimeout (TimeSpan) - How long to wait for a health check to repond. Defualt is 5 seconds.
* HealthChecksDirectory (string) - A directory to watch for dynamic health checks. Used for dynamic services that may not run all the time. Default is empty/turned off.
* ApiEndpoint (string) - The endpoint for the api. should be something like "/monitor-api". Default it's empty and turned off.
* HealthChecks (HealthCheckSetting[]) - The health checks to monitor. Default is empty.

The HealthCheckSetting properties are:
* Name (string) - The name to give the health check. Should be unique.
* Uri (string) - The Uri of the health check.
* Monitor (bool) - If we should actually monitor this health check. Default is true.

The example below shows how a complete options section would look in appsetting.json.

```json
{

    "HealthCheckMonitor" : {
        "EvaluationInterval" : "00:00:30",
        "InitialEvaluationDelay": "00:02:00",
        "HealthCheckTimeout": "00:00:05",
        "HealthChecksDirectory": "",
        "ApiEndpoint": "/monitor-api",
        "HealthChecks": [
            {
                "Name":"HealthCheck1",
                "Uri":"https://server1:port/health",
                "Monitor": true
            },
            {
                "Name":"HealthCheck2",
                "Uri":"https://server2:port/health",
                "Monitor": true
            }
        ]
    }

}
```

## Callbacks 
To access the callbacks there is HealthMonitorCallbacks. It will be added to the available services and provides two functions, one to add and one two remove callbacks.

eg:
```c#
internal class MyCallbacks
{
    private readonly HealthMonitorCallbacks _callbacks;

    MyCallbacks(HealthMonitorCallbacks callbacks)
    {
        _callbacks = callbacks;
    }

    public void AddCallbacks()
    {
        _callbacks.AddCallback("HealthCheck1", HealthMonitorStatus.Unhealthy, async (result) => {
            // This is called whenever HealthCheck1 reports being unhealthly
            // result contains the details available on the health check
            console.Writeline($"healthCheck1 has been unhealthy since {result.CurrentStatusStarted}");
        });

        _callbacks.AddCallback("HealthCheck1", HealthMonitorStatus.Healthy, async (result) => {
            // This is called whenever HealthCheck1 reports being healthly

            // The CurrentStatusCount is how many times in a row the current status has stayed the same
            if(result.CurrentStatusCount == 1)
            {
                console.Writeline($"healthCheck1 is back to being healthly");
            }
            
        });
    }

    public void RemoveCallbacks()
    {
        _callbacks.RemoveCallback("HealthCheck1", HealthMonitorStatus.Unhealthy);
        _callbacks.RemoveCallback("HealthCheck1", HealthMonitorStatus.Healthy);
    }
}
```

## Watched Directory
The watched directory is for use in the circumstances when you need to monitor an applications health, but you either don't know ahead of time what it Uri is going to be, or the service is not always on.

In these circumstances you can use dynamic files usually generated by the service your watching or through some other mechanism, and any healthchecks in these files will be immediatly loaded and monitored. The watcher loads it as an array so you can have more than one health check per file.

The files should end with the extension .json. and would look like this.

```json
[
    {
        "Name":"HealthDynamic",
        "Uri":"https://dynamic:port/health",
        "Monitor": true
    }
]
```

## Direct data access
At anytime you can access the results, and Health Checks by getting the HealthMonitorData from the services. With this singleton you can add or remove health checks programmatically, and access all the data collected.

```c#
public class MyData
{
    private readonly HealthMonitorData _data;

    public MyData(HealthMonitorData data)
    {
        _data = data;
    }

    public void AddHealthCheck()
    {
        var checks = new List<HealthCheckSetting>
        {
            new HealthCheckSetting{ Name = "MyCheck1", Uri = "https://myserver1/health" },
            new HealthCheckSetting{ Name = "MyCheck2", Uri = "Https://myserver2/health" }
        };

        _data.AddUpdate(checks);
    }

    public void GetSetting()
    {
        // All the healthchecks in the monitor
        var names = _data.HealthCheckNames();

        // the first setting
        var first = names[0];

        // the settings details
        var setting = _data.HealthCheckSetting(first);

        // the result data for the setting
        var result = _data.Result(name);
    }

    public void RemoveHealthCheck()
    {
        var names = new List<string> { "MyCheck1", "MyCheck2" };

        _data.Remove(names);
    }

    public void ReadResult()
    {
        var result = _data.
    }
}
```

# Health Check Monitor Client
The health check monitor client adds a couple of extensions to make setting up the clients easier.

When using the health checks you can add options to get the most detail for the HealthCheckMonitor these settings can be preset using the follwing line the Startup.Configure

```c#
public class Startup
{
    //...
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //...

        app.UseHealthChecks("/health", HealthCheckMonitorClientOptions.HealthCheckOptions());

        //...
    }
    //...
}
```

This sets a reponse writer with full json serialization, and alters the reponse code for "Degraded" to 218.

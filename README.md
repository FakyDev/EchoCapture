# EchoCapture

EchoCapture is a command-line application allowing you to capture your screen at an
interval time, with configurations of your choice, supported on Windows 10 and Windows 11.

# Requirements

.NET Desktop Runtime 5.x.x, is required to run the application.
[Download Here](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)

# Basic Configuration

### Set save folder location

The default save location of capture screen is found at `%localappdata%\EchoCapture`.
Below is an example on how to change the save folder to `C:\Games\Screenshots`.

```
.setting folder C:\Games\Screenshots
```

### Set capture screen interval

The default interval is 10s `(equivalent to 10000ms)`.
``` 1000ms = 1s```
Below is an example on how to change the capture screen interval.

```
.setting interval 40000
```

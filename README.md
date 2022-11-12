# EchoCapture

EchoCapture is a command-line application allowing you to capture your screen at an
interval time, with configurations of your choice, supported on Windows 10 and Windows 11.

# Requirements

.NET Desktop Runtime 5.x.x, is required to run the application.
[Download Here](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)

# Basic Configuration

### Set save folder location

The default save location of capture screen is found at ``%localappdata%\EchoCapture``.
Below is an example on how to change the save folder to ``C:\Games\Screenshots``.

```
.setting folder C:\Games\Screenshots
```

### Set capture screen interval

The default interval is 10s ``(equivalent to 10000ms)``.
Below is an example on how to change the capture screen interval.

``Note: 1000ms = 1s``

```
.setting interval 40000
```

### Display configuration

Below show the configuration that you are using and the presets configuration.

```
.information
```

# Capturing Screen

### Begin screen capture

Below start capturing your screen at the interval you set.

```
.task start
```

### Stop screen capture

Below end the capture screen task.
``Note: Closing the application will also end the task.``

```
.task stop
```

# Image Configuration

## General image setting

These settings are applied to each capture capture.

### Set image type

The capture screen can either be in ``png`` or ``jpeg``.
Below is an example on how to set the image type to ``png``.

[Click Here](https://undsgn.com/jpg-vs-png/#:~:text=The%20Difference%20between%20PNG%20and%20JPG&text=PNG%20stands%20for%20Portable%20Network,%2Dcalled%20%E2%80%9Clossy%E2%80%9D%20compression.)
to find the difference and select what fits you.

```
.imageSetting imageType png
```

### Set image-rescaling

For image-rescaling, you need first to enable it as shown below. 

```
.imageSetting rescaling true
```

Then you specify the rescaling resolution you desire, as below.
The rescaling resolution consists of ``Width x Height``.

```
.imageSetting rescalingResolution 1920 1080
```

Now each capture screen will be rescale to the rescaling resolution specified.
This can also be disabled, which is by default.


## Image Presets

There are three image presets available:
* high
* standard
* low

### Selecting image preset

The default preset is ``standard``.
You can select any preset as shown below.

```
.imageSetting preset high
```

### Customising image preset

Whenever you are customising an image preset, any changes made will be applied to the current chosen preset.

There are only two changes that can be made:
* Pixel format
* Image quality - only affect ```jpeg``` image-type.

#### Changing pixel format

You can change the pixel format for your capture screen, as shown below.

```
.imageSetting format Format48bppRgb
```

There are a bunch of pixel format available:
* Format16bppRgb555
* Format16bppRgb565
* Format24bppRgb
* Format32bppArgb
* Format32bppPArgb
* Format32bppRgb
* Format48bppRgb
* Format64bppArgb
* Format64bppPArgb

[Click here](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.pixelformat?view=dotnet-plat-ext-5.0) for more information about these pixel format.


#### Changing image quality (only jpeg)

This will only affect jpeg image-type capture screen.
The image quality ranges from ``0-100`` and ``100`` being with most details.
You can change to whatever you want as shown below.

```
.imageSetting jpgQuality 95
```

## Troubleshooting

### Capture screen are empty/black?

Try changing the [pixel format]("#Changing pixel format").


### No capture screen are saved?

Try changing the [save folder location]("#Set save folder location") to somewhere your user has permission.

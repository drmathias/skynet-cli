# Skynet CLI

A dotnet tool for uploading files to Sia Skynet.

![.NET Core](https://github.com/drmathias/skynet-cli/workflows/.NET%20Core/badge.svg?branch=master) [![NuGet](https://img.shields.io/nuget/v/skynet-cli)](https://www.nuget.org/packages/skynet-cli/)

## Prerequisites

* [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)

## Installation

```
dotnet tool install -g skynet-cli 
```

## Usage

```pwsh
PS C:\Users\adams> skynet -h
skynet:
  A command line tool to upload files to Sia Skynet

Usage:
  skynet [options] [command]

Options:
  --version         Show version information
  -?, -h, --help    Show help and usage information

Commands:
  upload <item>    Uploads an item to Sia Skynet
```

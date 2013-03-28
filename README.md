Roslyn-CSharp-Sample-Editor-2013-03
===================================

A WinForms C# source code editor demonstrates using Roslyn to parse C# syntax and highlight diagnostic messages.

This sample was cobbled together during the March Meetup session themed "Microsoft Roslyn" of the Toronto .NET Hackers http://www.meetup.com/Toronto-dotNET-Hackers/

Roslyn provides diagnotic information about compiler errors by way of line numbers, errors spans, etc and our program provides the highlight mechanism for that area of source code. We change the Console colors and use the RichTextBox control's text selection and colouring features. 

The purpose of the meetup session was to discover Roslyn technology.  This code is based on the Sept 2012 CTP release.

Platform: Visual Studio 2012, .NET 4.5, Console/WinForms hybrid app, Roslyn Sept 2012 CTP acquired through NuGet.

# PowerBuilder
PowerBuilder is a collection of specialty tools targeted at tasks that are only procedural for bim consultants and bim managers.  The primary objective is to power up consultants to help deliver projects more effectively. The secondary objective is providing bim consultants an environment to collaboratively learn the fundamentals of software development and the Revit API.

## What else goes here
Currently PowerBuilder just implements some basic patterns for linking IExternalCommands to the Revit interface.  Primarily procedural Command detection.  To have your command detected, attach it to the PowerBuilder.Commands namespace.

### TODO
- Begin adding commands
- Implement Unit Testing
- Implement FastForm*
- Multi-version build
- CI/CD
- Complete/update documentation
- move this todo list to some other tracker?

### *FastForm
FastForm is basically implementing the quick ui building aspects of Orkestra's OkPy in .NET using WinForms (depending on how much progress gets made, maybe this should be done in WPF.  I sort of like the clunky look of WinForms).  The idea here is to have sort of a wrapped implementation of several basic selection Form Controls so they just require the basic selection inputs, and the positioning and alignment is all handled procedurally.  This way contributors just have to associate the selection targets with the Form Control type they want to use, and pass the list of them into the FastForm which will enforce some basic alignment rules.  The main focus here is reducing the barriers from your basic "Hello World" tutorial and a useful command in Revit.

we have some brainstorming and notes in a miro here
https://miro.com/app/board/uXjVM8cCjbY=/

# PowerBuilder
PowerBuilder is a collection of specialty tools for bim consultants and bim managers.  The primary objective is to provide procedural solutions for the unique set of problems that are repeatable only in the consulting domain and not well covered by other trade specific extensions. The secondary objective is providing interested users an environment to collaboratively learn the fundamentals of software development and the Revit API.

## Adding new Commands
This is still sort of a work in progress.  To create a command that gets detected by the panel assembler, include these points
1. create a new branch for your command
2. copy 'Command1.cs' - this is currently sort of the 'boilerplate'/Template for new commands.  commands should typically follow something like Connect->Select->Execute.  Functions/procedures should be methods of the command so they can be independently called for unit testing. check back as this changes
3. assign it to the namespace PowerBuilder.Commands
4. your IExternalCommand class must have a static property "DisplayName" that can be found by the assembler. I had a thought to extend the IExternalCommand interface or make a custom child interface to make this mandatory.  I'd definitely like to figure out a quicker way to use some sort of Template with the essential parts and patterns that a new Command should follow.

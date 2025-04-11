# PowerBuilder
PowerBuilder is a collection of specialty tools for bim consultants and bim managers.  The primary objective is to provide procedural solutions for the unique set of problems that are repeatable only in the consulting domain and not well covered by other trade specific extensions. The secondary objective is providing interested users an environment to collaboratively learn the fundamentals of software development and the Revit API.

## Adding new Commands
This is still sort of a work in progress.  To create a command that gets detected by the panel assembler, include these points
1. create a new branch for your command
2. copy 'HelloWorld.cs' - this is currently sort of the 'boilerplate'/Template for new commands.  commands should typically follow something like Connect->Select->Execute.  Functions/procedures should be methods of the command so they can be independently called for unit testing. check back as this changes
3. assign it to the namespace PowerBuilder.Commands
4. Commands should use the IPowerCommand interface.  This extends IExternalCommand by requiring identity attributes used by the ribbon builder features.

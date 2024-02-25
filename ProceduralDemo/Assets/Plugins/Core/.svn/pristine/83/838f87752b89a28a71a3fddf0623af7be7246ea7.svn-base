https://docs.google.com/document/d/1XxJy-WExL62DYgYI385LCPAUpHXNBlCR54ZPCymanLg/edit?usp=sharing

---------- Shortcuts ----------

ctrl+shift+d - Game Data Import
ctrl+alt+d - Force Full Game Data Import

---------- Variant Schema ----------

How to use the data system?

Create StaticDataConfig scriptableObject, a Source folder, a Server folder, and a Client folder in one directory beside eachother.
	StaticDataConfig - Gives an access point to find all these directories you just created and also let you set which data sheets will get created into server jsons.
	Source - All data sheets a placed in here as well as all variant defining folders
	Client - Where all generated DB (scriptableObjects) for client use of data will be placed
	Server - Where all server jsons will be placed


How to create a data variant?

Creating a folder in Source creates a data variant. Data variants folders define the name of a variant and can be placed inside of other variants to child a variant to another variant. A child variant receives all its parents data, its parent’s parent data, and so on.


What are variants for?

Variants are for having modified versions of a base data source. Variants can be used to AB test data and/or having different builds with different data (ex. Netflix build vs FreeToPlay build)


DebugOptions.ImportVariants:
	True - Enable importing of data variants
	False - Only import base
	True & String Input - A single variant may be specified to import along with base. This is useful to reduce import times when only working on one variant.

---------- Data Import ----------

Tricky Things

Data importer will only look for private or protected fields and methods to enforce the data object being read only. Because of how reflection works the importer will not be able to find private fields and methods in a data objects base class. If your data class is meant to be inherited from you will want to make sure all of your fields and methods are protected.

Importing Blanks

Fields
	If a cell is left blank a field with the matching name will be reset to it's default value

Methods
	If a cell with a matching import method is blank the import method will not be called. However if the method parameter has a default value then the method WILL be called using that default value.

Field AND Method
	In the case that a cell is black and has a matching import method as well as a matching field, the fields default value is used.
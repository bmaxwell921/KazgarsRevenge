=======================================================================================
KAZGAR'S REVENGE CHUNK EDITOR
by: Brandon Maxwell
=======================================================================================

=======================================================================================
FOR USERS OF THE SYSTEM
=======================================================================================
	- The Chunk Editor requires Java 7 to compile and run. If you have problems
		make sure you have the right version of Java installed
	- The INTERFACE
		- The ChunkEditor interface is split up into 2 main sections: the first is the grid where you
			place things and the second is the list of things to place.
		- Inside the grid you start with a YELLOW CUBE in the top left corner - this is your location
		- The RED CUBE marks where doors should be placed.
			===NOTE=== doors are not placed there by default - you must place them
	- CONTROLS
		- Use the ARROW KEYS to move your current selection/yellow cube around the grid
		- Press ENTER to place your current selection at your current location
		- Press BACKSPACE to remove the item located at the top left block of your current selection
			===NOTE=== The easiest way to remove is by having the same selected item as the item you with
				to remove, then rotating that selection to match the item you wish to remove's location
		- Press R to rotate your current selection
	- CREATING BLOCKS
		- Blocks are the most basic object found inside of chunks. Default blocks
		 include FLOORS, DOORS, MOB SPAWNS, PLAYER SPAWNS, BOSS SPAWNS, KEYS, and CHESTS.
		- If you want to create a new block simply follow these steps:
			1) Navigate into the <PATH>/KRChunkEditor/blocks folder
			2) Copy and paste one of the other block files. 
			3) Change the name of this file to whatever you want it to be, then open 
				it in a text editor. Inside you will find the JSON representation of a block. 
			4) The only thing you need to change is the "name" attribute. Simply change 
				that name to whatever you named the file, and you're done. 
			5) If you want, you can also add an image for that block in the <PATH>/KRChunkEditor/img/blocks folder. 
				The image should be 100x100 to fit with everything else. Images are scaled to 25x25 by the program when displaying.
	- CREATING ROOMS
		- Rooms are the second most basic object found in chunks. They exist primarily as a
			convenience for placing configurations of blocks.
		- To create a new room follow these steps:
			1) Start the ChunkEditor program by double clicking on ChunkEditor.jar
			2) Click the NEW ROOM button at the bottom of the screen
				- If necessary, load another room to build off of
			3) In the dialog window that opens, place blocks as necessary.
			4) Save the new room in the <PATH>/KRChunkEditor/rooms folder
				===NOTE=== IF YOU DON'T SAVE IT HERE, IT WON'T BE READ BY THE CHUNK EDITOR
				===NOTE=== IF YOU GIVE IT THE SAME NAME AS ANOTHER ROOM, YOU WILL HAVE TO RESTART
					THE PROGRAM FOR CHANGES TO TAKE EFFECT
			5) The program will automagically create an image representation of the room using
				block images already defined.
				===NOTE=== If an image isn't found for a given block it will use a block with a '?' inside it
	- CREATING CHUNKS
		- Chunks are the most complex objects in the ChunkEditor (duh). They are made up of rooms.
		- Follow these steps to create a new chunk
			1) Start the ChunkEditor program by double clicking on the ChunkEditor.jar file
				- if necessary, load another room to build off of
			2) Place rooms as necessary
			3) Save the chunk wherever you like.

=======================================================================================
FOR USERS OF THE JSON OUTPUT
=======================================================================================			
	- Things to keep in mind
		- The top left corner of a chunk is (0, 0), x increases going RIGHT, y increases going DOWN.
		- Block locations are relative to the location of a room. 
			- For instance if we have blocks at (0, 0), (1, 0), and (0, 1) in a room located at
			(2, 3), the blocks' real coordinates are (2, 3), (3, 3), and (2, 4).
			- The same goes for rooms, but the chunk's location is always (0, 0) so it doesn't matter
		- The "name" attribute of a chunk should match the chunk model's file name	
	- Building enemy path graph
		- Should be simple enough, blocks are nodes, edges connect between adjacent blocks.
		- After blocks inside rooms get connected to each other we just need to connect the 
			doors of adjacent rooms

=======================================================================================
FAQS
=======================================================================================	
	1) OMG THE PROGRAM WON'T RUN!
		- If they don't exist already, try making the these folders:
			<PATH>/KRChunkEditor/rooms
			<PATH>/KRChunkEditor/blocks
			<PATH>/KRChunkEditor/img/rooms
			<PATH>/KRChunkEditor/img/blocks
		- Make sure you are running Java 7 and not Java 6. Open a terminal an type
			"java -version"
		  It should say something like: "java version 1.7..."
		- If you know what you're doing, open up a terminal and run: "java -jar ChunkEditor.jar"
			If you can figure out what's up based on the output great, otherwise tell me/show me
			what is shown in the terminal.
	2) I CAN'T REMOVE STUFF!
		- Try selecting the room you want to remove (from the right side panel) and then rotate it
			to match the room you want to remove. The location it chooses to remove at isn't user 
			user friendly, but I wrote that in like 10 minutes so bite me.	

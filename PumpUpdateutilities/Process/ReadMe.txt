ues to compare the versions from construction folder and the release notes, also check with the corresponding release notes section for versions

parameter 1: release title eg: 05 Oct 2018



Input:
	Release Date

Stage 1:
	Compare version from construction folder to HTML table and content
	Display and pause when there is a difference

Stage 2:
	Compare the content of the release notes from construction folder to HG // may not be necessary 

	Check Pending Release Notes for anything in Ready to release
	Copy the section to temp file and search for the driver in PumpUpdate release notes 
		If the driver already has release notes 
			check the existing notes,
				If the node in pending release is not available in pumpUpdate -> Add it
				If it has already -> Next notes
			check the version from the pending release notes and pump update notes and the construction folder, choose the version in construction folder, then use the one in construction folder if the versions are different -> Mark the notes as RED
		If the driver has no release notes -> Not found in pump update note
			Add all notes -> Mark as ORANGE

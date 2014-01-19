package main.kazgarsrevenge.util;

import java.io.File;

import javax.swing.filechooser.FileFilter;

	// Filter to only allow json files
	public class JsonFolderFilter extends FileFilter {

		private static final String ACCEPTED_EXT = ".json";
		
		@Override
		public boolean accept(File f) {
			if (f.isDirectory()) {
				return true;
			}
			
			final String name = f.getName();
			return name.endsWith(ACCEPTED_EXT);
		}

		@Override
		public String getDescription() {
			return ACCEPTED_EXT;
		}	
	}
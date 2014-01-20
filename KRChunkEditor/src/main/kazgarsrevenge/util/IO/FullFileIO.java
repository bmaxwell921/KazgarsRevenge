package main.kazgarsrevenge.util.IO;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;

/**
 * Class used to read a file in its entirety.
 * I got tired of writing the same code...
 * @author Brandon
 *
 */
public class FullFileIO {
	
	/**
	 * Method used to read the file at the given path entirely into the returned value.
	 * If there are errors, they'll be reported in errors.
	 * @param path
	 * @param sb
	 * @return
	 */
	public static StringBuilder readEntirely(File file, StringBuilder errors) {
		try (BufferedReader br = new BufferedReader(new FileReader(file))) {
			StringBuilder ret = new StringBuilder();
			String read = "";
			while ((read = br.readLine()) != null) {
				ret.append(read);
			}
			return ret;
		} catch (IOException ioe) {
			errors.append(String.format("Unable to open given file: %s\n", file.getName()));
			errors.append(ioe.getMessage());
			return null;
		}
	}
}

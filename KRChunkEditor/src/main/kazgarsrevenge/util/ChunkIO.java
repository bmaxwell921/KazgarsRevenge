package main.kazgarsrevenge.util;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;

import main.kazgarsrevenge.model.IChunk;
import main.kazgarsrevenge.model.chunks.impl.DefaultChunk;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

/**
 * Class used to load and save chunks
 * @author Brandon
 *
 */
public class ChunkIO {
	
	/**
	 * Loads the chunk found at the given path, or null if there are problems.
	 * If null is returned, errors will contain the problems
	 * @param path
	 * @param errors
	 * @return
	 */
	public static IChunk loadChunk(File chunkFile, StringBuilder errors) {
		Gson gson = new Gson();
		StringBuilder chunk = new StringBuilder();
		
		try (BufferedReader br = new BufferedReader(new FileReader(chunkFile))) {
			String read = "";
			while ((read = br.readLine()) != null) {
				chunk.append(read);
			}
		} catch (IOException ioe) {
			errors.append(String.format("Unable to open given file: %s\n", chunkFile.getName()));
			errors.append(ioe.getMessage());
			return null;
		}
		
		try {
			return gson.fromJson(chunk.toString(), DefaultChunk.class);
		} catch (Exception jse) {
			errors.append("Unable to parse json\n");
			errors.append(jse.getMessage());
			return null;
		}
	}
	
	/**
	 * Saves the given chunk to the given location. If there are errors messages, they can be
	 * found in errors
	 * @param chunk
	 * @param saveLoc
	 * @param errors
	 */
	public static void saveChunk(IChunk chunk, File saveLoc, StringBuilder errors) {
		try (BufferedWriter bw = new BufferedWriter(new FileWriter(saveLoc))) {
			bw.write(chunk.getJsonRep());
			bw.flush();
		} catch (IOException ioe) {
			errors.append(String.format("Unable to save file to: %s", saveLoc.getName()));
			errors.append(ioe.getMessage());
		}
	}
	
}

package main.kazgarsrevenge.util;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import java.util.Set;

import main.kazgarsrevenge.model.IRoom;
import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;

/**
 * Class to read the Rooms.json file and create objects from it
 * @author bmaxwell
 *
 */
public class RoomFileUtil {

	private static final String ROOMS_FILE_PATH = "." + File.separatorChar + "Rooms.json";
	
	// Mapping of known rooms. Keys are the room name
	private static Map<String, DefaultRoom> knownRooms;
	
	public static void readKnownRooms() {
		knownRooms = new HashMap<String, DefaultRoom>();
		
		try (BufferedReader br = new BufferedReader(new FileReader(ROOMS_FILE_PATH))) {
			StringBuilder sb = new StringBuilder();
			String read = "";
			
			while ((read = br.readLine()) != null) {
				sb.append(read);
			}
			
			JsonParser parser = new JsonParser();
			JsonArray rooms = parser.parse(sb.toString()).getAsJsonArray();
			
			RoomFactory rf = new RoomFactory();
			
			for (JsonElement room :  rooms) {
				DefaultRoom roomObj = rf.getRoom(room);
				knownRooms.put(roomObj.getRoomName(), roomObj);
			}
			
		} catch (IOException ioe) {
			System.out.println("Problems reading Rooms.json");
			ioe.printStackTrace();
		}
	}
	
	public static Set<String> getRoomNames() {
		return knownRooms.keySet();
	}
	
	public static DefaultRoom createRoom(String name) {
		if (!knownRooms.containsKey(name)) throw new IllegalArgumentException(String.format("Unknown room name: %s", name));
		return (DefaultRoom) knownRooms.get(name).clone();
	}
}

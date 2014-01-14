package main.kazgarsrevenge.util;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.lang.reflect.Type;
import java.util.Map;

import main.kazgarsrevenge.model.rooms.impl.DefaultRoom;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonArray;
import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;

/**
 * Factory class used to create IRooms
 * @author Brandon
 *
 *
 *
 *
 */
public class RoomFactory {
	
	public static void main(String[] args) throws IOException { 

//		JsonObject blockOne = new JsonObject();
//		blockOne.addProperty("x", 0);
//		blockOne.addProperty("y", 0);
//		blockOne.addProperty("Rotation", "Zero");
//		
//		JsonObject blockTwo = new JsonObject();
//		blockTwo.addProperty("x", 0);
//		blockTwo.addProperty("y", 0);
//		blockTwo.addProperty("Rotation", "Zero");
//		
//		JsonArray floorArr = new JsonArray();
//		floorArr.add(blockOne);
//		floorArr.add(blockTwo);
//		
//		JsonObject roomOne = new JsonObject();
//		roomOne.addProperty("Name", "Room1");
//		roomOne.add("Floor", floorArr);
//		
//		JsonArray file = new JsonArray();
//		file.add(roomOne);
//		
//		BufferedWriter bw = new BufferedWriter(new FileWriter("./Rooms.txt"));
//		bw.write(file.toString());
//		bw.close();

		
		// TODO do this elsewhere
		BufferedReader br = new BufferedReader(new FileReader("./Rooms.txt"));
		StringBuilder sb = new StringBuilder();
		String read = "";
		
		while ((read = br.readLine()) != null) {
			sb.append(read);
		}		
		JsonParser parser = new JsonParser();
		JsonArray rooms = parser.parse(sb.toString()).getAsJsonArray();
		
		for (JsonElement room : rooms) {
			System.out.println(new RoomFactory().getRoom(room));
		}
		
	}
	
	public DefaultRoom getRoom(JsonElement roomJson) {
		GsonBuilder builder = new GsonBuilder();
		builder.registerTypeAdapter(DefaultRoom.class, new RoomDeserializer());
		Gson gson = builder.create();
		return gson.fromJson(roomJson, DefaultRoom.class);		
	}
	
	/**
	 * Converts the json into a DefaultRoom object
	 * @author Brandon
	 *
	 */
	private static class RoomDeserializer implements JsonDeserializer<DefaultRoom> {

		// TODO finish implementing this
		@Override
		public DefaultRoom deserialize(final JsonElement json, final Type type,
				final JsonDeserializationContext context) throws JsonParseException {
			final JsonObject obj = json.getAsJsonObject();
			System.out.println("In here!");
			for (Map.Entry<String, JsonElement> prop : obj.entrySet()) {
				
			}
			
			return null;
		}
		
	}
}

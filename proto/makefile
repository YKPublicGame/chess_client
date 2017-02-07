
all:Room.cs Login.cs Table.cs

Room.cs:room.proto
	./protogen.exe -i:room.proto -o:Room.cs

Login.cs:login.proto
	./protogen.exe -i:login.proto -o:Login.cs

Table.cs:table.proto
	./protogen.exe -i:table.proto -o:Table.cs

clean:
	rm *.cs

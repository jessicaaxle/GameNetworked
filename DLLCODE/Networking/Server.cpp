#include "pch.h"
#include "Server.h"
#define BUFLEN 512

#define SERVERLOG std::cout<<"Server: "

void Server::InitServer(const char* p_IP, const int p_port)
{
	SERVERLOG << "Init server stated\n";

	if (InitWinsock() == 0)
	{
		IP = p_IP;
		port = p_port;

		StartListen();
	}
	else
	{
		SERVERLOG << "Could not start winsock\n";
	}

}

void Server::Cleanup()
{
	listening = false;
	closesocket(sock);
}

int Server::InitWinsock()
{
	WSAData data;
	WORD version = MAKEWORD(2, 2);

	return WSAStartup(version, &data);
}

void Server::StartListen()
{
	// First we need a socket
	if (InitSocket() == 0)
	{
		listening = true;
		std::thread t(&Server::RecvLoop, this);
		t.detach();
	}
	else
		SERVERLOG << "Could not init socket\n";
}

int Server::InitSocket(){
	sock = socket(AF_INET, SOCK_DGRAM, 0);
	if (sock == INVALID_SOCKET)
	{
		return -1;
	}
	return BindSocket();
}

int Server::BindSocket()
{
	sockaddr_in hint;
	hint.sin_addr.S_un.S_addr = INADDR_BROADCAST;
	hint.sin_family = AF_INET;
	hint.sin_port = htons(port);
	inet_pton(AF_INET, IP, &hint.sin_addr);

	if (bind(sock, (sockaddr*)&hint, sizeof(hint)) == SOCKET_ERROR)
	{
		return -1;
	}
	return 0;
}

void Server::RecvLoop()
{
	SERVERLOG << "Started listening SERVER\n";

	sockaddr_in clientAddr;
	int clientLength = sizeof(clientAddr);
	char buf[256];

	while (listening)
	{
		if (!listening)
			return;

		ZeroMemory(buf, 256);

		int numBytesReceived = recvfrom(sock, buf, 256, 0, (sockaddr*)&clientAddr, &clientLength);

		if (numBytesReceived > 0)
		{
			ProcessMessage(buf, clientAddr);
		}
	}
}

void Server::ProcessMessage(const char* msg, sockaddr_in clientAddr)
{
	SERVERLOG << "Received message: " << msg << std::endl;

	if (IsNewUser(clientAddr))
	{
		ClientConnectionData temp;
		temp.name = msg;
		temp.status = "online";
		temp.id = clients.size();
		temp.addr = clientAddr;

		std::string msgToSend = "i;" + std::to_string(temp.id);

		SendPacketToTargetClient(msgToSend.c_str(), clientAddr);

		// Send a message with all of the other client's data
		for (int i = 0; i < clients.size(); i++)
		{
			SendPacketToTargetClient(clients[i].to_message().c_str(), clientAddr);
		}

		clients.push_back(temp);

		BroadcastMessage(temp.to_message().c_str(), clientAddr);
	}
	else
	{
		if (msg[0] == 's')
		{
			FindClientByAddress(clientAddr)->status = &msg[1];

			BroadcastMessage(msg, clientAddr);
		}
		if (msg[0] == 'm')
		{
			BroadcastMessage(msg, clientAddr);
		}
		if (msg[0] == 'v')
		{
			BroadcastMessage(msg, clientAddr);
		}
		else
		{
			// Validate the packet
			// Discard it? 
			BroadcastMessage(msg, clientAddr);
		}
	}

}

//void Server::ProcessPosition(const char* msg, sockaddr_in clientAddr)
//{
//	SERVERLOG << "PROCESS POS: " << msg << std::endl;
//
//	float tx = 0.0f;
//	float ty = 0.0f;
//
//	char buf[BUFLEN];
//	struct sockaddr_in fromAddr;
//	int fromlen;
//	fromlen = sizeof(fromAddr);
//
//	memset(buf, 0, BUFLEN);
//
//	int bytes_received = -1;
//	int sError = -1;
//
//	std::cout << "Received WORK: " << buf << std::endl;
//
//	std::string tmp = buf;
//	std::size_t pos = tmp.find("@"); //creates a position to separate data
//	tmp = tmp.substr(0, pos - 1); //Pos - 1 is tx
//	tx = std::stof(tmp, NULL); //Convert to float
//	tmp = buf; //Reset the temp variable 
//	tmp = tmp.substr(pos + 1); //pos + 1 is ty
//	ty = std::stof(tmp, NULL);
//	//Can be done for other axes?
//
//	std::cout << "tx: " << tx << std::endl;
//	std::cout << "ty: " << ty << std::endl;
//}

bool Server::IsNewUser(sockaddr_in clientAddr)
{
	for (int i = 0; i < clients.size(); i++)
	{
		if (clients[i].addr.sin_port == clientAddr.sin_port)
		{
			return false;
		}
	}
	return true;
}

void Server::SendPacketToTargetClient(const char* msg, const sockaddr_in clientAddr)
{
	sendto(sock, msg, std::string(msg).length(), 0, (sockaddr*)&clientAddr, sizeof(clientAddr));
}

// Send a message to all clients except for the one specified
void Server::BroadcastMessage(const char* msg, const sockaddr_in clientAddrExcept)
{
	SERVERLOG << "Sending: " << msg << std::endl;
	for (int i = 0; i < clients.size(); i++)
	{
		//if (clients[i].addr.sin_port != clientAddrExcept.sin_port)
		//{
			SendPacketToTargetClient(msg, clients[i].addr);
		//}
	}
}

ClientConnectionData* Server::FindClientByAddress(sockaddr_in addr)
{
	for (int i = 0; i < clients.size(); i++)
	{
		if (clients[i].addr.sin_port == addr.sin_port)
		{
			return &clients[i];
		}
	}
	return nullptr; // be sad
}
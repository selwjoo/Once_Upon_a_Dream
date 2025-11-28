import json
from channels.generic.websocket import AsyncWebsocketConsumer

class GameConsumer(AsyncWebsocketConsumer):
    async def connect(self):
        self.match_id = self.scope['url_route']['kwargs']['match_id']
        self.room_name = f"match_{self.match_id}"

        await self.channel_layer.group_add(self.room_name, self.channel_name)
        await self.accept()
        
        print(f"[연결] match_id: {self.match_id}, channel: {self.channel_name}")

    async def disconnect(self, close_code):
        await self.channel_layer.group_discard(self.room_name, self.channel_name)
        print(f"[연결 해제] match_id: {self.match_id}")

    async def receive(self, text_data):
        data = json.loads(text_data)
        
        print(f"[수신] match: {self.match_id}, type: {data.get('type')}, from: {data.get('username')}")
        
        # 자신의 channel_name을 데이터에 포함
        await self.channel_layer.group_send(
            self.room_name,
            {
                "type": "game_message",
                "payload": data,
                "sender_channel": self.channel_name  # 발신자 식별
            }
        )

    async def game_message(self, event):
        # 자기가 보낸 메시지는 다시 받지 않음
        if event.get("sender_channel") == self.channel_name:
            return
        
        print(f"[전송] to channel: {self.channel_name}, type: {event['payload'].get('type')}")
        await self.send(text_data=json.dumps(event["payload"]))
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
        
        # spawn_request는 별도 처리
        if data['type'] == 'spawn_request':
            import random
            x = random.uniform(-5, 5)  # minX, maxX 범위
            y = random.uniform(-3, 3)  # minY, maxY 범위
            
            # 모든 클라이언트에게 브로드캐스트
            await self.channel_layer.group_send(
                self.room_name,  # ← 여기! room_group_name이 아니라 self.room_name
                {
                    'type': 'spawn_message',
                    'x': x,
                    'y': y
                }
            )
            return  # 여기서 끝!
        
        # 일반 메시지는 기존 방식대로
        await self.channel_layer.group_send(
            self.room_name,
            {
                "type": "game_message",
                "payload": data,
                "sender_channel": self.channel_name
            }
        )

    async def game_message(self, event):
        # 자기가 보낸 메시지는 다시 받지 않음
        if event.get("sender_channel") == self.channel_name:
            return
        
        print(f"[전송] to channel: {self.channel_name}, type: {event['payload'].get('type')}")
        await self.send(text_data=json.dumps(event["payload"]))

    async def spawn_message(self, event):
        await self.send(text_data=json.dumps({
            'type': 'spawn',
            'x': event['x'],
            'y': event['y']
        }))
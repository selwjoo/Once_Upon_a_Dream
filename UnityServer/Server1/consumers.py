import json
from channels.generic.websocket import AsyncWebsocketConsumer
import random

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
        msg_type = data.get('type')

        print(f"[수신] room: {self.room_name}, type: {msg_type}, from: {data.get('username')}")

        # === 스폰 요청 ===
        if msg_type == 'spawn_request':

            x = random.uniform(-10, 10)
            y = random.uniform(-10, 10)
            await self.channel_layer.group_send(
                self.room_name,
                {
                    'type': 'spawn_message',
                    'x': x,
                    'y': y
                }
            )
            return

        # === 점수 업데이트 ===
        if msg_type == 'scoreGame1_update':
            await self.channel_layer.group_send(
                self.room_name,
                {
                    'type': 'broadcast_message',
                    'message': text_data
                }
            )
            return

        # === 타이머 업데이트 ===
        if msg_type == 'timer_update':
            await self.channel_layer.group_send(
                self.room_name,
                {
                    'type': 'broadcast_message',
                    'message': text_data
                }
            )
            return

        # === 역할 업데이트 ===
        if msg_type in ['role_select', 'chase_roles']:
            await self.channel_layer.group_send(
                self.room_name,
                {
                    'type': 'broadcast_message',
                    'message': text_data
                }
            )
            return

        # === 일반 메시지 ===
        await self.channel_layer.group_send(
            self.room_name,
            {
                "type": "game_message",
                "payload": data,
                "sender_channel": self.channel_name
            }
        )

    # ================= 브로드캐스트 핸들러 =================
    async def broadcast_message(self, event):
        message = event["message"]
        await self.send(text_data=message)

    async def spawn_message(self, event):
        # x, y 전달
        await self.send(text_data=json.dumps({
            'type': 'spawn',
            'x': event['x'],
            'y': event['y']
        }))

    async def game_message(self, event):
        payload = event["payload"]
        sender_channel = event["sender_channel"]
        # 자신이 보낸 메시지는 제외
        if sender_channel != self.channel_name:
            await self.send(text_data=json.dumps(payload))
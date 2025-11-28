from django.shortcuts import render

# Create your views here.
from django.http import JsonResponse
from django.views.decorators.csrf import csrf_exempt
from django.contrib.auth import authenticate
import json, random
from django.contrib.auth.models import User

waiting_players = []
active_matches = {}    # 매칭된 방 정보
# 방 정보 저장 (임시, 실제로는 DB에)
rooms = {}  

@csrf_exempt
def unity_data(request):
    if request.method == 'POST':
        data = json.loads(request.body)
        print(data)  # 서버 콘솔에서 데이터 확인
        return JsonResponse({"status": "success", "received": data})
    return JsonResponse({"error": "POST 요청만 가능"})

@csrf_exempt
def unity_login(request):
    if request.method == "POST":
        data = json.loads(request.body)
        username = data.get("username")
        password = data.get("password")
        
        user = authenticate(username=username, password=password)
        if user is not None:
            # 로그인 성공
            return JsonResponse({"success": True, "message": "로그인 성공"})
        else:
            return JsonResponse({"error": False, "message": "아이디 또는 비번 틀림"})
    return JsonResponse({"error": False, "message": "POST 요청만 허용"}, status=400)

@csrf_exempt
def unity_register(request):
    if request.method == "POST":
        data = json.loads(request.body)
        username = data.get("username")
        password = data.get("password")

        if User.objects.filter(username=username).exists():
            return JsonResponse({"error": False, "message": "이미 계정이 있습니다."})

        # 새 사용자 생성
        user = User.objects.create_user(username=username, password=password)
        user.save()

        return JsonResponse({"success": True, "message": "회원가입 성공"})
    
    return JsonResponse({"error": False, "message": "POST 요청만 허용"}, status=400)


@csrf_exempt
def unity_ready(request):
    if request.method == "POST":
        data = json.loads(request.body)
        username = data.get("username")

        # 이미 매칭된 플레이어면 기존 방 정보 반환
        if username in active_matches:
            match_info = active_matches[username]
            return JsonResponse({"match": True, "room": match_info["room"], "players": match_info["players"]})

        # 아직 큐에 없는 경우만 추가
        if username not in waiting_players:
            waiting_players.append(username)

        print("현재 대기 큐:", waiting_players)

        # 대기 큐에 2명 이상이면 매칭
        if len(waiting_players) >= 2:
            player1 = waiting_players.pop(0)
            player2 = waiting_players.pop(0)
            room_id = f"room_{random.randint(1000,9999)}"

            match_info = {"room": room_id, "players": [player1, player2]}

            # 두 플레이어 모두에게 active_matches에 저장
            active_matches[player1] = match_info
            active_matches[player2] = match_info

             # rooms에도 생성
            rooms[room_id] = {"players": [player1, player2], "roles": {}}

            return JsonResponse({"match": True, "room": room_id, "players": [player1, player2]})

        # 대기 중
        return JsonResponse({"match": False})

    return JsonResponse({"success": False}, status=400)

@csrf_exempt
def choose_role(request):
    if request.method == "POST":
        data = json.loads(request.body)
        username = data.get("username")
        room_id = data.get("room")
        chosen_role = data.get("chosenRole")

        if room_id not in rooms:
            return JsonResponse({"success": False, "message": "방이 존재하지 않습니다."})

        room = rooms[room_id]

        # 이미 선택된 역할이면 배정 안됨
        if chosen_role in room["roles"].values():
            return JsonResponse({"success": False, "message": "이미 선택된 역할입니다."})

        # 역할 배정
        room["roles"][username] = chosen_role

        # 두 역할 모두 선택됐는지 확인
        has_roleA = "RoleA" in room["roles"].values()
        has_roleB = "RoleB" in room["roles"].values()
        both_selected = has_roleA and has_roleB

        # ★ 상태를 room dict에 저장 ★
        room["both_selected"] = room.get("both_selected", False) or both_selected

        return JsonResponse({
            "success": True,
            "role": chosen_role,
            "both_selected": room["both_selected"]
        })
    
    return JsonResponse({"success": False, "message": "POST 요청만 허용"}, status=400)

@csrf_exempt
def get_role(request):
    if request.method == "POST":
        data = json.loads(request.body)
        username = data.get("username")
        room_id = data.get("room")

        if room_id not in rooms:
            return JsonResponse({"success": False, "message": "방이 존재하지 않습니다."})

        room = rooms[room_id]
        role = room["roles"].get(username, None)  # 없으면 None 반환

        return JsonResponse({
            "success": True if role else False,
            "role": role
        })

    return JsonResponse({"success": False, "message": "POST 요청만 허용"}, status=400)

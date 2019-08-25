#define _CRT_SECURE_NO_WARNINGS
#include <Windows.h>
#include <conio.h>
#include <stdio.h>
#include <string>
#include <psapi.h>
#include <intrin.h> 
#include <algorithm>
#include <vector>
#include <fstream>
#include <codecvt>
#include <tlhelp32.h>
#include <winternl.h>
#include <winnt.h>
#include "cProcInfo.h"

/*
// Explained in p. 2 below
void NTAPI tls_callback(PVOID DllHandle, DWORD dwReason, PVOID)
{
	if (dwReason == DLL_THREAD_ATTACH)
	{
		MessageBox(0, "DLL_THREAD_ATTACH", "DLL_THREAD_ATTACH", 0);
	}

	if (dwReason == DLL_PROCESS_ATTACH)
	{
		MessageBox(0, "DLL_PROCESS_ATTACH", "DLL_PROCESS_ATTACH", 0);
	}
}

#ifdef _WIN64
#pragma comment (linker, "/INCLUDE:_tls_used")  // See p. 1 below
#pragma comment (linker, "/INCLUDE:tls_callback_func")  // See p. 3 below
#else
#pragma comment (linker, "/INCLUDE:__tls_used")  // See p. 1 below
#pragma comment (linker, "/INCLUDE:_tls_callback_func")  // See p. 3 below
#endif

// Explained in p. 3 below
#ifdef _WIN64
#pragma const_seg(".CRT$XLF")
EXTERN_C const
#else
#pragma data_seg(".CRT$XLF")
EXTERN_C
#endif
PIMAGE_TLS_CALLBACK tls_callback_func = tls_callback;
#ifdef _WIN64
#pragma const_seg()
#else
#pragma data_seg()
#endif //_WIN64
*/ //for testing injector


#define INRANGE(x,a,b)    (x >= a && x <= b)
#define getBits( x )    (INRANGE((x&(~0x20)),'A','F') ? ((x&(~0x20)) - 'A' + 0xa) : (INRANGE(x,'0','9') ? x - '0' : 0))
#define getByte( x )    (getBits(x[0]) << 4 | getBits(x[1]))
DWORD FindPattern(std::string moduleName, std::string pattern) {
	const char* pat = pattern.c_str();
	DWORD firstMatch = 0;
	DWORD rangeStart = (DWORD)GetModuleHandleA(moduleName.c_str());
	MODULEINFO miModInfo;
	GetModuleInformation(GetCurrentProcess(), (HMODULE)rangeStart, &miModInfo, sizeof(MODULEINFO));
	DWORD rangeEnd = rangeStart + miModInfo.SizeOfImage;
	for (DWORD pCur = rangeStart; pCur < rangeEnd; pCur++) {
		if (!*pat) return firstMatch;
		if (*(PBYTE)pat == '\?' || *(BYTE*)pCur == getByte(pat)) {
			if (!firstMatch) firstMatch = pCur;
			if (!pat[2]) return firstMatch;
			if (*(PWORD)pat == '\?\?' || *(PBYTE)pat != '\?') pat += 3;
			else pat += 2;
		} else {
			pat = pattern.c_str();
			firstMatch = 0;
		}
	}
	return NULL;
}

DWORD FindValue(std::string moduleName, DWORD value){
	DWORD rangeStart = (DWORD)GetModuleHandleA(moduleName.c_str());
	MODULEINFO miModInfo;
	GetModuleInformation(GetCurrentProcess(), (HMODULE)rangeStart, &miModInfo, sizeof(MODULEINFO));
	DWORD rangeEnd = rangeStart + miModInfo.SizeOfImage;
	for (DWORD pCur = rangeStart; pCur < rangeEnd; pCur += sizeof(DWORD)) {
		if (*(DWORD*)pCur == value) return pCur;
	}
	return 0;
}

DWORD FindValueEx(DWORD * value) {
	SYSTEM_INFO	si;
	GetSystemInfo(&si);
	DWORD pCur = 0;
	MEMORY_BASIC_INFORMATION info;
	while (pCur < (DWORD)si.lpMaximumApplicationAddress) {
		if (!VirtualQueryEx(GetCurrentProcess(), (LPCVOID)pCur, &info, sizeof(info))) return 0;
		if (info.State & MEM_COMMIT && PAGE_READWRITE == info.Protect && info.Type & MEM_PRIVATE) {
			for (pCur = (DWORD)info.BaseAddress; pCur < (DWORD)info.BaseAddress + (DWORD)info.RegionSize; pCur += sizeof(pCur)) {
				__try {
					if (*(DWORD*)pCur == *value && pCur != (DWORD)value) return pCur; // it was finding itself on stack and the original variable lamao
				} __except (EXCEPTION_EXECUTE_HANDLER) {};
			}
		}
		pCur = (DWORD)info.BaseAddress + (DWORD)info.RegionSize;
	}
	return 0;
}

void SuspendOtherThreads() {
	HANDLE hThreadSnap = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	if (!hThreadSnap) return;
	THREADENTRY32 te32;
	te32.dwSize = sizeof(THREADENTRY32);
	if (!Thread32First(hThreadSnap, &te32)) {
		CloseHandle(hThreadSnap);
		return;
	}
	DWORD curID = GetCurrentProcessId();
	DWORD curTID = GetCurrentThreadId();
	do {
		if (te32.th32OwnerProcessID == curID && te32.th32ThreadID != curTID) {
			HANDLE thread = OpenThread(THREAD_SUSPEND_RESUME, 0, te32.th32ThreadID);
			if (thread) {
				SuspendThread(thread);
				CloseHandle(thread);
			}
		}
	} while (Thread32Next(hThreadSnap, &te32));
	CloseHandle(hThreadSnap);
}

void ResumeAllThreads() {
	HANDLE hThreadSnap = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	if (!hThreadSnap) return;
	THREADENTRY32 te32;
	te32.dwSize = sizeof(THREADENTRY32);
	if (!Thread32First(hThreadSnap, &te32)) {
		CloseHandle(hThreadSnap);
		return;
	}
	DWORD curID = GetCurrentProcessId();
	DWORD curTID = GetCurrentThreadId();
	do {
		if (te32.th32OwnerProcessID == curID && te32.th32ThreadID != curTID) {
			HANDLE thread = OpenThread(THREAD_SUSPEND_RESUME, 0, te32.th32ThreadID);
			if (thread) {
				ResumeThread(thread);
				CloseHandle(thread);
			}
		}
	} while (Thread32Next(hThreadSnap, &te32));
	CloseHandle(hThreadSnap);
}

typedef void(__thiscall* OnLoginBtnClick)(void* thisPtr);
typedef int(__thiscall* f_SetText)(void* thisPtr, const char* name, char* test, bool translate);
typedef int(__thiscall* f_AuthCodeEnteredHandler)(void* thisPtr, const char* buttonName);
typedef int(__thiscall* f_RetardedFunction2)(void* thisPtr);

std::u32string to_utf32(std::string s) {
	std::wstring_convert<std::codecvt_utf8<int32_t>, int32_t> convert;
	auto asInt = convert.from_bytes(s);
	return std::u32string(reinterpret_cast<char32_t const*>(asInt.data()), asInt.length());
}

DWORD WINAPI DebugThread(LPVOID arg) {
	/*CONTEXT context;
	ZeroMemory(&context, sizeof(context));
	context.ContextFlags = CONTEXT_FULL;
	HANDLE thread = (HANDLE)arg;
	DWORD lastEIP = 0;
	while (true) {
		Sleep(100);
		GetThreadContext(thread, &context);
		if (lastEIP == context.Eip) {
			ResumeAllThreads();
			return 0;
		}
		lastEIP = context.Eip;
	}*/
	Sleep(1000);
	ResumeAllThreads();
	return FALSE;
}

enum ERRORS{
	ERR_OK = 0,
	ERR_CLICK_HANDLER_PATERN_FAIL = 1,
	ERR_AUTCH_CODE_HANDLER_PATERN_FAIL_1,
	ERR_AUTCH_CODE_HANDLER_PATERN_FAIL_2,
};

BOOL CALLBACK MyEnumWindowsProc(HWND hwnd, LPARAM lparam)
{
	char classname[100];
	GetClassName(hwnd, classname, sizeof(classname));
	if (strstr(classname, "vguiPopupWindow") != NULL && IsWindowVisible(hwnd))* (DWORD*)lparam = 1;
	return TRUE;
}

DWORD WINAPI MainThread(LPVOID hModule) {
	printf("injected\n");
	char* username = (char*)(DWORD)hModule;
	char* password = (char*)((DWORD)username + strlen(username) + 1);
	char* guard = (char*)((DWORD)password + strlen(password) + 1);
	while (!GetModuleHandleA("steamui.dll")) Sleep(100);
	printf("trying window\n");
	DWORD found = 0;
	while (EnumWindows(MyEnumWindowsProc, (LPARAM)&found) && !found) Sleep(100);
	printf("found window\n");
	Sleep(2000);
	DWORD clickHandler = FindPattern("steamui.dll", "55 8B EC 53 8B 1D ?? ?? ?? ?? 56 57 8B 7D 08 8B F1 68 FF FF FF 7F 68 ?? ?? ?? ?? 57 FF D3 83 C4 0C 85 C0 75 12 8D 8E EC FE FF FF E8 ?? ?? ?? ?? 5F 5E 5B 5D C2 04 00");
	DWORD loginBtnClickHandler = clickHandler + 48 + *(int*)(clickHandler + 44);
	DWORD CSteamLoginDialogVtable = FindValue("steamui.dll", clickHandler) - 0x108;//index 66
	DWORD CSteamLoginDialogObject = 0;
	while (!(CSteamLoginDialogObject = FindValueEx(&CSteamLoginDialogVtable))) Sleep(100);
	DWORD NotificationFrame = CSteamLoginDialogObject + 0x114;
	while (!*(DWORD*)NotificationFrame) Sleep(100);
	DWORD SetText = *(DWORD*)(*(DWORD*)NotificationFrame + 0x220);//index 88
	printf("ch: %X\nlh: %X\nvt: %X\nwo: %X\nnf: %X\nst: %X\n", clickHandler, loginBtnClickHandler, CSteamLoginDialogVtable, CSteamLoginDialogObject, NotificationFrame, SetText);
	((f_SetText)SetText)((void*)NotificationFrame, "UserNameEdit", username, 0);
	((f_SetText)SetText)((void*)NotificationFrame, "PasswordEdit", password, 0);
	((OnLoginBtnClick)loginBtnClickHandler)((void*)CSteamLoginDialogObject);
	if (!strlen(guard)) return 0;
	DWORD AuthCodeEnteredHandler = FindPattern("steamui.dll", "55 8B EC 81 EC 80 00 00 00 56 68 FF FF FF 7F 68 ?? ?? ?? ?? FF 75 08 8B F1 FF 15");
	DWORD TwoFactorCodeChallengeVtable = FindValue("steamui.dll", AuthCodeEnteredHandler) - 0xA0;//index 28
	DWORD TwoFactorCodeChallengeObject = 0;
	while (!(TwoFactorCodeChallengeObject = FindValueEx(&TwoFactorCodeChallengeVtable))) Sleep(100);
	Sleep(2000);
	DWORD AuthInputField = *(DWORD*)(TwoFactorCodeChallengeObject + 0x218);
	DWORD AuthUtf32text = AuthInputField + 0xF8;
	DWORD AutchTextLen = AuthInputField + 0x104;
	printf("\nah: %X\nvt: %X\nao: %X\nif: %X\ntxt: %X\nl: %X\n", AuthCodeEnteredHandler, TwoFactorCodeChallengeVtable, TwoFactorCodeChallengeObject, AuthInputField, AuthUtf32text, AutchTextLen);
	DWORD retardedshit = FindPattern("steamui.dll", "E8 ?? ?? ?? ?? 8D 4D ?? 51 8B C8 8B 10 FF 92 ?? ?? ?? ?? B9 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 06 8B CE C6 86 10 02 00 00 01 FF 90 ?? ?? ?? ?? 5E 8B E5 5D C2 04 00");
	DWORD GetRetardObject = retardedshit + 5 + *(int*)(retardedshit + 1); //call ???????? + offset
	DWORD RetardObject = ((DWORD (__stdcall*)())GetRetardObject)();
	DWORD RetardedObject2 = *(DWORD*)(retardedshit + 20); //mov ecx, ????????
	DWORD ReterdedFuction2 = retardedshit + 29 + *(int*)(retardedshit + 25);
	DWORD RetardedBool = TwoFactorCodeChallengeObject + 0x210;
	DWORD RetardedFunction3 = *(DWORD*)(TwoFactorCodeChallengeVtable + 0x27C);//index 159
	printf("\ngro: %X\nro: %X\nro2: %X\nrf2: %X\nrf3: %X\n", GetRetardObject, RetardObject, RetardedObject2, ReterdedFuction2, RetardedFunction3);
	printf("\nXD: %X\n", (DWORD)&MainThread);
	std::u32string str = to_utf32(guard);
	char* utf32str = new char[str.size() * sizeof(char32_t)];
	memcpy(utf32str, str.data(), str.size() * sizeof(char32_t));
	*(DWORD*)AuthUtf32text = (DWORD)utf32str;
	*(DWORD*)AutchTextLen = str.size();
	SuspendOtherThreads();
	CreateThread(0, 0, DebugThread, (LPVOID)GetCurrentThread(), 0, 0);
	(*(void(__thiscall * *)(void*, char*))(*(DWORD*)RetardObject + 0xD0))((void*)RetardObject, (char*)guard);
	((f_RetardedFunction2)ReterdedFuction2)((void*)RetardedObject2);
	*(BYTE*)RetardedBool = (BYTE)1;
	((int(__thiscall*)(void*))RetardedFunction3)((void*)TwoFactorCodeChallengeObject);
	printf("done\n");
	Sleep(1000);
	FreeLibraryAndExitThread((HMODULE)hModule, 1);
	return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
	if (ul_reason_for_call == DLL_PROCESS_ATTACH) {
		//AllocConsole();
		//freopen("CONOUT$", "w", stdout);
		//freopen("CONIN$", "r", stdin);
		CreateThread(0, 0, MainThread, hModule, 0, 0);
	}
	return TRUE;
}


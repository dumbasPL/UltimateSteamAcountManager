#pragma once
#include <Windows.h>
#include <winternl.h>
#include <winnt.h>
#include "cProcInfo.h"

cProcInfo::cProcInfo() {
	mu32_DataSize = 1000;
	mp_Data = NULL;
	mf_NtQueryInfo = NULL;
}
cProcInfo::~cProcInfo() {
	if (mp_Data) LocalFree(mp_Data);
}

	// Capture all running processes and all their threads.
	// returns an API or NTSTATUS Error code or zero if successfull
DWORD cProcInfo::Capture() {
	if (!mf_NtQueryInfo) {
		mf_NtQueryInfo = (t_NtQueryInfo)GetProcAddress(GetModuleHandleA("NtDll.dll"), "NtQuerySystemInformation");
		if (!mf_NtQueryInfo)
			return GetLastError();
	}

	// This must run in a loop because in the mean time a new process may have started 
	// and we need more buffer than u32_Needed !!
	while (true) {
		if (!mp_Data) {
			mp_Data = (BYTE*)LocalAlloc(LMEM_FIXED, mu32_DataSize);
			if (!mp_Data)
				return GetLastError();
		}

		ULONG u32_Needed = 0;
		NTSTATUS s32_Status = mf_NtQueryInfo(SystemProcessInformation, mp_Data, mu32_DataSize, &u32_Needed);

		if (s32_Status == STATUS_INFO_LENGTH_MISMATCH) // The buffer was too small
		{
			mu32_DataSize = u32_Needed + 4000;
			LocalFree(mp_Data);
			mp_Data = NULL;
			continue;
		}
		return s32_Status;
	}
}

	// Searches a process by a given Process Identifier
	// Capture() must have been called before!
SYSTEM_PROCESS* cProcInfo::FindProcessByPid(DWORD u32_PID) {
	if (!mp_Data) {
		return NULL;
	}

	SYSTEM_PROCESS* pk_Proc = (SYSTEM_PROCESS*)mp_Data;
	while (TRUE) {
		if ((DWORD)(DWORD_PTR)pk_Proc->UniqueProcessId == u32_PID)
			return pk_Proc;

		if (!pk_Proc->NextEntryOffset)
			return NULL;

		pk_Proc = (SYSTEM_PROCESS*)((BYTE*)pk_Proc + pk_Proc->NextEntryOffset);
	}
}

SYSTEM_THREAD* cProcInfo::FindThreadByTid(SYSTEM_PROCESS* pk_Proc, DWORD u32_TID) {
	if (!pk_Proc) {
		return NULL;
	}

	// The first SYSTEM_THREAD structure comes immediately after the SYSTEM_PROCESS structure
	SYSTEM_THREAD* pk_Thread = (SYSTEM_THREAD*)((BYTE*)pk_Proc + sizeof(SYSTEM_PROCESS));

	for (DWORD i = 0; i < pk_Proc->ThreadCount; i++) {
		if (pk_Thread->ClientID.UniqueThread == (HANDLE)(DWORD_PTR)u32_TID)
			return pk_Thread;

		pk_Thread++;
	}
	return NULL;
}

DWORD cProcInfo::IsThreadSuspended(SYSTEM_THREAD* pk_Thread, BOOL* pb_Suspended) {
	if (!pk_Thread)
		return ERROR_INVALID_PARAMETER;

	*pb_Suspended = (pk_Thread->ThreadState == Waiting &&
		pk_Thread->WaitReason == Suspended);
	return 0;
}

// Based on the 32 bit code of Sven B. Schreiber on:
// http://www.informit.com/articles/article.aspx?p=22442&seqNum=5
#pragma once

typedef LONG NTSTATUS;

#define STATUS_SUCCESS              ((NTSTATUS) 0x00000000)
#define STATUS_INFO_LENGTH_MISMATCH ((NTSTATUS) 0xC0000004)

enum KWAIT_REASON {
	Executive,
	FreePage,
	PageIn,
	PoolAllocation,
	DelayExecution,
	Suspended,
	UserRequest,
	WrExecutive,
	WrFreePage,
	WrPageIn,
	WrPoolAllocation,
	WrDelayExecution,
	WrSuspended,
	WrUserRequest,
	WrEventPair,
	WrQueue,
	WrLpcReceive,
	WrLpcReply,
	WrVirtualMemory,
	WrPageOut,
	WrRendezvous,
	Spare2,
	Spare3,
	Spare4,
	Spare5,
	Spare6,
	WrKernel,
	MaximumWaitReason
};

enum THREAD_STATE {
	Running = 2,
	Waiting = 5,
};

#pragma pack(push,8)

//struct CLIENT_ID {
//	HANDLE UniqueProcess; // Process ID
//	HANDLE UniqueThread;  // Thread ID
//};

// http://www.geoffchappell.com/studies/windows/km/ntoskrnl/api/ex/sysinfo/thread.htm
// Size = 0x40 for Win32
// Size = 0x50 for Win64
struct SYSTEM_THREAD {
	LARGE_INTEGER KernelTime;
	LARGE_INTEGER UserTime;
	LARGE_INTEGER CreateTime;
	ULONG         WaitTime;
	PVOID         StartAddress;
	CLIENT_ID     ClientID;           // process/thread ids
	LONG          Priority;
	LONG          BasePriority;
	ULONG         ContextSwitches;
	THREAD_STATE  ThreadState;
	KWAIT_REASON  WaitReason;
};

struct VM_COUNTERS // virtual memory of process
{
	ULONG_PTR PeakVirtualSize;
	ULONG_PTR VirtualSize;
	ULONG     PageFaultCount;
	ULONG_PTR PeakWorkingSetSize;
	ULONG_PTR WorkingSetSize;
	ULONG_PTR QuotaPeakPagedPoolUsage;
	ULONG_PTR QuotaPagedPoolUsage;
	ULONG_PTR QuotaPeakNonPagedPoolUsage;
	ULONG_PTR QuotaNonPagedPoolUsage;
	ULONG_PTR PagefileUsage;
	ULONG_PTR PeakPagefileUsage;
};

// http://www.geoffchappell.com/studies/windows/km/ntoskrnl/api/ex/sysinfo/process.htm
// See also SYSTEM_PROCESS_INROMATION in Winternl.h
// Size = 0x00B8 for Win32
// Size = 0x0100 for Win64
struct SYSTEM_PROCESS {
	ULONG          NextEntryOffset; // relative offset
	ULONG          ThreadCount;
	LARGE_INTEGER  WorkingSetPrivateSize;
	ULONG          HardFaultCount;
	ULONG          NumberOfThreadsHighWatermark;
	ULONGLONG      CycleTime;
	LARGE_INTEGER  CreateTime;
	LARGE_INTEGER  UserTime;
	LARGE_INTEGER  KernelTime;
	UNICODE_STRING ImageName;
	LONG           BasePriority;
	PVOID          UniqueProcessId;
	PVOID          InheritedFromUniqueProcessId;
	ULONG          HandleCount;
	ULONG          SessionId;
	ULONG_PTR      UniqueProcessKey;
	VM_COUNTERS    VmCounters;
	ULONG_PTR      PrivatePageCount;
	IO_COUNTERS    IoCounters;   // defined in winnt.h
};

#pragma pack(pop)

typedef NTSTATUS(WINAPI* t_NtQueryInfo)(SYSTEM_INFORMATION_CLASS, PVOID, ULONG, PULONG);

class cProcInfo {
public:
	cProcInfo();
	virtual ~cProcInfo();
	DWORD Capture();
	SYSTEM_PROCESS* FindProcessByPid(DWORD u32_PID);
	SYSTEM_THREAD* FindThreadByTid(SYSTEM_PROCESS* pk_Proc, DWORD u32_TID);
	DWORD IsThreadSuspended(SYSTEM_THREAD* pk_Thread, BOOL* pb_Suspended);
private:
	BYTE* mp_Data;
	DWORD mu32_DataSize;
	t_NtQueryInfo mf_NtQueryInfo;
};
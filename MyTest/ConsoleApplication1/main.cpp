#include "lua.hpp"
#include <iostream>
#include <cstring>
#include <chrono>

#define DAENGINE_API


struct Vec3
{
    float x, y, z;
};

lua_State* s_lua = nullptr;
int test_int = 5;

extern "C"
{
    // this function returns the lua state
    __declspec(dllexport) lua_State*  YieldLua()
    {
        return s_lua;
    }

    // create and stores the lua state
    __declspec(dllexport) void CreateLuaState()
    {
        s_lua = luaL_newstate();
        luaL_openlibs(s_lua);
    }

    // a call back to demo calling from C#
    int CallMe(lua_State* l)
    {
        std::cout << "This function is in C...\n";
        return 0;
    }

    __declspec(dllexport) void CallCS()
    {
        std::cout << "I'm in C, Trying to Call a C# function called CSharpCallback...\n";
        lua_getglobal(s_lua, "CSharpCallback");
        lua_pcall(s_lua, 0, 0, 0);
    }

    __declspec(dllexport) void CCallCSStressTest()
    {
        for (int i = 0; i < 10000; ++i)
        {
            lua_getglobal(s_lua, "CSharpCallbackEmpty");
            lua_pcall(s_lua, 0, 0, 0);
        }
    }

    __declspec(dllexport) int empty_func(lua_State* l)
    {
        return 0;
    }

    // initialize lua state
    __declspec(dllexport) void InitLuaState()
    {
        lua_pushcfunction(s_lua, CallMe);
        lua_setglobal(s_lua, "CallBackInC");

        lua_pushnumber(s_lua, test_int);
        lua_setglobal(s_lua, "test_int");

        lua_pushcfunction(s_lua, empty_func);
        lua_setglobal(s_lua, "CCallbackEmpty");
    }
}

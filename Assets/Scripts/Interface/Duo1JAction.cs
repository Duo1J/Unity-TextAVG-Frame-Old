﻿namespace Duo1J
{
    //可自行实现此接口挂在到AVGFrame同物体下以自动注入
    public interface Duo1JAction : Duo1JAVG
    {
        void GetButtonChoose(string eventTag, Choose choose);
    }
}
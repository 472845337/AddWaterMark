﻿using System;
using System.Threading;

namespace AddWaterMark.DataBase {
    /// <summary>
    /// 使用using代替lock操作的对象，可指定写入和读取锁定模式
    /// </summary>
    public sealed class ClsLock {
        #region 内部类

        /// <summary>
        /// 利用IDisposable的using语法糖方便的释放锁定操作内部类
        /// </summary>
        private readonly struct Lock : IDisposable {
            /// <summary>
            /// 读写锁对象
            /// </summary>
            private readonly ReaderWriterLockSlim _lock;
            /// <summary>
            /// 是否为写入模式
            /// </summary>
            private readonly bool _isWrite;
            /// <summary>
            /// 利用IDisposable的using语法糖方便的释放锁定操作构造函数
            /// </summary>
            /// <param name="rwl">读写锁</param>
            /// <param name="isWrite">写入模式为true，读取模式为false</param>
            public Lock(ReaderWriterLockSlim rwl, bool isWrite) {
                _lock = rwl;
                _isWrite = isWrite;
            }
            /// <summary>
            /// 释放对象时退出指定锁定模式
            /// </summary>
            public void Dispose() {
                if (_isWrite) {
                    if (_lock.IsWriteLockHeld) {
                        _lock.ExitWriteLock();
                    }
                } else {
                    if (_lock.IsReadLockHeld) {
                        _lock.ExitReadLock();
                    }
                }
            }
        }

        /// <summary>
        /// 空的可释放对象，免去了调用时需要判断是否为null的问题内部类
        /// </summary>
        private class Disposable : IDisposable {
            /// <summary>
            /// 空的可释放对象
            /// </summary>
            public static readonly Disposable Empty = new Disposable();
            /// <summary>
            /// 空的释放方法
            /// </summary>
            public void Dispose() { }
        }

        #endregion

        /// <summary>
        /// 读写锁
        /// </summary>
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();
        /// <summary>
        /// 使用using代替lock操作的对象，可指定写入和读取锁定模式构造函数
        /// </summary>
        public ClsLock() {
            Enabled = true;
        }
        /// <summary>
        /// 是否启用，当该值为false时，Read()和Write()方法将返回 Disposable.Empty
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary> 
        /// 进入读取锁定模式，该模式下允许多个读操作同时进行，
        /// 退出读锁请将返回对象释放，建议使用using语块,
        /// Enabled为false时，返回Disposable.Empty，
        /// 在读取或写入锁定模式下重复执行，返回Disposable.Empty;
        /// </summary>
        public IDisposable Read() {
            if (Enabled == false || _lockSlim.IsReadLockHeld || _lockSlim.IsWriteLockHeld) {
                return Disposable.Empty;
            } else {
                _lockSlim.EnterReadLock();
                return new Lock(_lockSlim, false);
            }
        }

        /// <summary> 
        /// 进入写入锁定模式,该模式下只允许同时执行一个读操作，
        /// 退出读锁请将返回对象释放，建议使用using语块，
        /// Enabled为false时，返回Disposable.Empty，
        /// 在写入锁定模式下重复执行，返回Disposable.Empty
        /// </summary>
        /// <exception cref="NotImplementedException">读取模式下不能进入写入锁定状态</exception>
        public IDisposable Write() {
            if (Enabled == false || _lockSlim.IsWriteLockHeld) {
                return Disposable.Empty;
            } else if (_lockSlim.IsReadLockHeld) {
                throw new NotImplementedException("ReadMode can't get into writelock state");
            } else {
                _lockSlim.EnterWriteLock();
                return new Lock(_lockSlim, true);
            }
        }
    }

}
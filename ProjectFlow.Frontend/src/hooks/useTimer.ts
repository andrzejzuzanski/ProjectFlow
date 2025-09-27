import { useState, useEffect, useRef } from "react";

export function useTimer(isActive: boolean, startTime?: string) {
  const [seconds, setSeconds] = useState(0);
  const intervalRef = useRef<number | null>(null);

  useEffect(() => {
    if (isActive && startTime) {
      const start = new Date(
        startTime + (startTime.endsWith("Z") ? "" : "Z")
      ).getTime();

      const updateTimer = () => {
        const now = Date.now();
        const diff = Math.floor((now - start) / 1000);
        setSeconds(Math.max(0, diff));
      };

      updateTimer();
      intervalRef.current = window.setInterval(updateTimer, 1000);
    } else {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
      setSeconds(0);
    }

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [isActive, startTime]);

  const formatTime = (totalSeconds: number): string => {
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const secs = totalSeconds % 60;

    if (hours > 0) {
      return `${hours}:${minutes.toString().padStart(2, "0")}:${secs
        .toString()
        .padStart(2, "0")}`;
    }
    return `${minutes}:${secs.toString().padStart(2, "0")}`;
  };

  return {
    seconds,
    formattedTime: formatTime(seconds),
  };
}

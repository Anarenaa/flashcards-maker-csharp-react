import { useState, useEffect } from "react";

/**
 * Allows delaying the appearance of the loading state (e.g., by 300ms),
 * to avoid UI flickering during fast requests.
 */
export function useDelayedLoading(isLoading, delay = 300) {
  const [showLoading, setShowLoading] = useState(false);

  useEffect(() => {
    let timer;
    if (isLoading) {
      timer = setTimeout(() => {
        setShowLoading(true);
      }, delay);
    } else {
      setShowLoading(false);
    }
    return () => clearTimeout(timer);
  }, [isLoading, delay]);

  return showLoading;
}
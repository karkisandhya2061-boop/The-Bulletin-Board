import React, { useState } from 'react';
import {
  detectNavigationIntent,
  navigateToSection,
  generateNavigationResponse,
  generateFollowUpQuestion,
  generateChatResponse
} from '../utils/navigationIntent';

/**
 * AI Navigation Chatbot Component
 * Handles both navigation and friendly chat interactions
 */
export default function Chatbot() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([
    {
      id: 1,
      text: "👋 Hi! I'm your navigation assistant. I can take you to any news section or just chat with you!\n\nTry saying:\n• 'Take me to politics'\n• 'Show tech news'\n• 'Politics' (just the section name)\n• Or chat with me like a friend!",
      sender: 'bot',
      timestamp: new Date(),
      isWelcome: true
    }
  ]);
  const [inputValue, setInputValue] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    
    if (!inputValue.trim() || isTyping || isProcessing) return;

    const userInput = inputValue.trim();

    const userMessage = {
      id: messages.length + 1,
      text: userInput,
      sender: 'user',
      timestamp: new Date()
    };

    setMessages(prev => [...prev, userMessage]);
    setInputValue('');
    setIsTyping(true);
    setIsProcessing(true);

    try {
      console.log('🔍 Detecting intent for:', userInput);
      const intentResult = await detectNavigationIntent(userInput);
      
      console.log('📊 Intent result:', {
        intent: intentResult.intent,
        confidence: intentResult.confidence,
        method: intentResult.method
      });

      let botResponseText = '';
      let shouldNavigate = false;

      if (intentResult.intent && intentResult.confidence >= 0.5) {
        // HIGH CONFIDENCE - NAVIGATE
        botResponseText = generateNavigationResponse(intentResult.intent);
        shouldNavigate = true;

        setTimeout(() => {
          navigateToSection(intentResult.intent);
        }, 800);
      } else if (intentResult.intent && intentResult.confidence > 0) {
        // MEDIUM CONFIDENCE
        botResponseText = generateNavigationResponse(intentResult.intent) + '\n\n(Or we can just chat if you like!)';
        shouldNavigate = false;
      } else {
        // NO NAVIGATION INTENT - General chat
        botResponseText = generateChatResponse(userInput);
      }

      const botMessage = {
        id: messages.length + 2,
        text: botResponseText,
        sender: 'bot',
        timestamp: new Date(),
        intent: intentResult.intent,
        confidence: intentResult.confidence
      };

      setMessages(prev => [...prev, botMessage]);

    } catch (error) {
      console.error('Chatbot error:', error);
      const errorMessage = {
        id: messages.length + 2,
        text: "Oops! Something went wrong. Try again!",
        sender: 'bot',
        timestamp: new Date(),
        isError: true
      };
      setMessages(prev => [...prev, errorMessage]);
    } finally {
      setIsTyping(false);
      setIsProcessing(false);
    }
  };

  return (
    <>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="chatbot-toggle"
        title="Open Navigation Assistant"
        aria-label="Open chatbot"
      >
        💬
      </button>

      {isOpen && (
        <div className="chatbot-window">
          <div className="chatbot-header">
            <h3>🧭 Navigation Assistant</h3>
            <button
              onClick={() => setIsOpen(false)}
              className="chatbot-close"
              title="Close"
              aria-label="Close chatbot"
            >
              ×
            </button>
          </div>

          <div className="chatbot-messages">
            {messages.map(msg => (
              <div 
                key={msg.id} 
                className={`chatbot-message ${msg.sender}${msg.isError ? ' error' : ''}`}
              >
                <div className="message-bubble">
                  {msg.text.split('\n').map((line, idx) => (
                    <div key={idx}>{line}</div>
                  ))}
                </div>
                <span className="message-time">
                  {msg.timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
              </div>
            ))}
            
            {isTyping && (
              <div className="chatbot-message bot">
                <div className="message-bubble typing">
                  <span></span>
                  <span></span>
                  <span></span>
                </div>
              </div>
            )}
          </div>

          <form onSubmit={handleSendMessage} className="chatbot-input-form">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="e.g., 'show me tech news'..."
              className="chatbot-input"
              disabled={isTyping || isProcessing}
              autoComplete="off"
            />
            <button
              type="submit"
              className="chatbot-send-btn"
              disabled={isTyping || isProcessing || !inputValue.trim()}
              title="Send message"
              aria-label="Send message"
            >
              {isProcessing ? '⏳' : '➤'}
            </button>
          </form>
        </div>
      )}
    </>
  );
}

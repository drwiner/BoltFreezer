;;;
;;; The Blocks domain!
;;;
(define (domain blocks)
  (:requirements :adl )
  (:types location thing - object
		  agent item - thing
          block - item
		  steeringagent - agent)
  (:constants )
  (:predicates (at ?thing - thing ?location - location)
			   (has ?agent - agent ?item - item)
			   (occupied ?location - location)
			   (adjacent ?location - location ?newlocation - location)
			   (freehands ?agent - agent))

  (:action move
    :parameters   (?agent - steeringagent ?from - location ?to - location)
	:precondition (and (not (occupied ?to))
					   (not (= ?from ?to))
					   (adjacent ?from ?to)
                       (at ?agent ?from))
	:effect       (and (not (at ?agent ?from))
					   (not (occupied ?from))
                       (at ?agent ?to)
					   (occupied ?to)
					   ))
	
	
  (:action pickup
    :parameters   (?taker - agent ?block - block ?location - location ?takerLocation - location)
	:precondition (and (not (= ?location ?takerLocation))
					   (at ?taker ?takerLocation)
					   (at ?block ?location)
					   (adjacent ?location ?takerLocation)
					   (freehands ?taker))
	:effect       (and (not (at ?block ?location))
					   (not (occupied ?location))
					   (not (freehands ?taker))
					   (has ?taker ?block)
					   ))
	
  (:action putdown
    :parameters   (?putter - agent ?thing - block ?agentlocation - location ?newlocation - location)
	:precondition (and (not (occupied ?newlocation))
					   (not (= ?agentlocation ?newlocation))
					   (at ?putter ?agentlocation)
					   (has ?putter ?thing)
					   (adjacent ?agentlocation ?newlocation))
	:effect       (and (not (has ?putter ?thing))
					   (at ?thing ?newlocation)
					   (occupied ?newlocation)
					   (freehands ?putter)
					   ))
)
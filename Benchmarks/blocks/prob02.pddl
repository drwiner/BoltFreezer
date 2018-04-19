;;;
;;; The blocks problem!
;;;
(define (problem 01)
  (:domain blocks)
  (:objects boid - steeringagent
            l1 l2 l3 l4 - location
			block1 block2 - block)
  (:init (at block1 l1)
		 (at block2 l2)
		 (at boid l3)
		 (freehands boid)
		 (occupied l1)
		 (occupied l2)
		 (occupied l3)
		 (adjacent l1 l2)
		 (adjacent l2 l1)
		 (adjacent l2 l3)
		 (adjacent l3 l2)
		 (adjacent l2 l4)
		 (adjacent l4 l2))
  (:goal (at block1 l3)))
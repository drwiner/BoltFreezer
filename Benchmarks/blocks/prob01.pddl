;;;
;;; The blocks problem!
;;;
(define (problem 01)
  (:domain blocks)
  (:objects boid - steeringagent
            l0 l1 l2 l3 l4 l5 - location
			block1 block2 - block)
  (:init (at block1 l0)
		 (at block2 l1)
		 (at boid l2)
		 (freehands boid)
		 (occupied l0)
		 (occupied l1)
		 (occupied l2)
		 (adjacent l0 l1)
		 (adjacent l1 l0)
		 (adjacent l1 l2)
		 (adjacent l2 l1)
		 (adjacent l1 l3)
		 (adjacent l3 l1)
		 (adjacent l3 l4)
		 (adjacent l4 l3)
		 (adjacent l4 l2)
		 (adjacent l2 l4)
		 (adjacent l5 l2)
		 (adjacent l2 l5)
		)
  (:goal (at block1 l5)))